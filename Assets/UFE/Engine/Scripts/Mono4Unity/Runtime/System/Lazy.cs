//
// Lazy.cs
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//  Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2009 Novell
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//#if NET_4_0 || MOONLIGHT

using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Diagnostics;

/// <summary>
/// 惰性初始化（Lazy&lt;T&gt;）。
/// <para>用途：实现"按需延迟初始化"的值类型包装——首次访问 Value 时才执行工厂创建实例，</para>
/// <para>支持三种线程安全模式（LazyThreadSafetyMode.None/PublicationOnly/ExecutionAndPublication）。</para>
/// <para>从 Mono 移植，供 UFE 线程安全延迟初始化使用。</para>
/// </summary>
namespace System
{
	/// <summary>
	/// 惰性初始化泛型类：首次访问 Value 时按工厂延迟创建值。
	/// </summary>
	[SerializableAttribute]
	[ComVisibleAttribute(false)]
	//[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class Lazy<T> 
	{
		/// <summary>已初始化的值。</summary>
		T value;
		/// <summary>是否已初始化。</summary>
		bool inited;
		/// <summary>线程安全模式。</summary>
		LazyThreadSafetyMode mode;
		/// <summary>值工厂委托。</summary>
		Func<T> factory;
		/// <summary>同步监视器对象（线程安全模式用）。</summary>
		object monitor;
		/// <summary>初始化时抛出的异常缓存。</summary>
		Exception exception;
		
		/// <summary>
		/// 默认构造函数（ExecutionAndPublication 线程安全模式，使用默认值创建）。
		/// </summary>
		public Lazy ()
			: this (LazyThreadSafetyMode.ExecutionAndPublication)
		{
		}
		
		/// <summary>
		/// 构造函数（指定值工厂，使用 ExecutionAndPublication 线程安全模式）。
		/// </summary>
		/// <param name="valueFactory">值工厂。</param>
		public Lazy (Func<T> valueFactory)
			: this (valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
		{
		}
		
		/// <summary>
		/// 构造函数（指定是否线程安全）。
		/// </summary>
		/// <param name="isThreadSafe">是否线程安全。</param>
		public Lazy (bool isThreadSafe)
			: this (() => Activator.CreateInstance<T> (), isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
		{
		}
		
		/// <summary>
		/// 构造函数（指定值工厂与是否线程安全）。
		/// </summary>
		/// <param name="valueFactory">值工厂。</param>
		/// <param name="isThreadSafe">是否线程安全。</param>
		public Lazy (Func<T> valueFactory, bool isThreadSafe)
			: this (valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
		{
		}
		
		/// <summary>
		/// 构造函数（指定线程安全模式，使用默认值创建）。
		/// </summary>
		/// <param name="mode">线程安全模式。</param>
		public Lazy (LazyThreadSafetyMode mode)
			: this (() => Activator.CreateInstance<T> (), mode)
		{
		}
		
		
		
		/// <summary>
		/// 构造函数（指定值工厂与线程安全模式）。
		/// </summary>
		/// <param name="valueFactory">值工厂。</param>
		/// <param name="mode">线程安全模式。</param>
		public Lazy (Func<T> valueFactory, LazyThreadSafetyMode mode)
		{
			if (valueFactory == null)
				throw new ArgumentNullException ("valueFactory");
			this.factory = valueFactory;
			if (mode != LazyThreadSafetyMode.None)
				monitor = new object ();
			this.mode = mode;
		}
		
		// Don't trigger expensive initialization
		/// <summary>
		/// 惰性值属性：首次访问时初始化，已缓存则直接返回；初始化异常会缓存并重新抛出。
		/// </summary>
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		public T Value {
			get {
				if (inited)
					return value;
				if (exception != null)
					throw exception;
				
				return InitValue ();
			}
		}
		
		/// <summary>
		/// 执行实际初始化：按线程安全模式（None/PublicationOnly/ExecutionAndPublication）创建并缓存值。
		/// </summary>
		/// <returns>初始化后的值。</returns>
		T InitValue ()
		{
			Func<T> init_factory;
			T v;
			
			switch (mode) {
			case LazyThreadSafetyMode.None:
				init_factory = factory;
				if (init_factory == null) 
					throw exception = new InvalidOperationException ("The initialization function tries to access Value on this instance");
				try {
					factory = null;
					v = init_factory ();
					value = v;
					Thread.MemoryBarrier ();
					inited = true;
				} catch (Exception ex) {
					exception = ex;
					throw;
				}
				break;
				
			case LazyThreadSafetyMode.PublicationOnly:
				init_factory = factory;
				
				//exceptions are ignored
				if (init_factory != null)
					v = init_factory ();
				else
					v = default (T);
				
				lock (monitor) {
					if (inited)
						return value;
					value = v;
					Thread.MemoryBarrier ();
					inited = true;
					factory = null;
				}
				break;
				
			case LazyThreadSafetyMode.ExecutionAndPublication:
				lock (monitor) {
					if (inited)
						return value;
					
					if (factory == null)
						throw exception = new InvalidOperationException ("The initialization function tries to access Value on this instance");
					
					init_factory = factory;
					try {
						factory = null;
						v = init_factory ();
						value = v;
						Thread.MemoryBarrier ();
						inited = true;
					} catch (Exception ex) {
						exception = ex;
						throw;
					}
				}
				break;
				
			default:
				throw new InvalidOperationException ("Invalid LazyThreadSafetyMode " + mode);
			}
			
			return value;
		}
		
		/// <summary>
		/// 值是否已创建。
		/// </summary>
		public bool IsValueCreated {
			get {
				return inited;
			}
		}
		
		/// <summary>
		/// 转换为字符串（已创建返回值的字符串，否则返回提示文本）。
		/// </summary>
		public override string ToString ()
		{
			if (inited)
				return value.ToString ();
			else
				return "Value is not created";
		}
	}		
}

//#endif

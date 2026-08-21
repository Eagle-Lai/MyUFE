// 
// LazyInitializer.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;

/// <summary>
/// 惰性初始化器（LazyInitializer）。
/// <para>用途：从 Mono 移植——提供无需创建 Lazy&lt;T&gt; 对象的轻量惰性初始化辅助方法，</para>
/// <para>支持基于 Interlocked 的快速初始化与基于锁/标志的线程安全初始化。</para>
/// </summary>
namespace System.Threading
{
	/// <summary>
	/// 惰性初始化静态类：确保目标字段在首次访问时被初始化。
	/// </summary>
	public static class LazyInitializer
	{
		/// <summary>
		/// 确保目标已初始化（使用默认构造函数，Interlocked 快速路径）。
		/// </summary>
		/// <typeparam name="T">目标类型。</typeparam>
		/// <param name="target">目标引用（ref）。</param>
		/// <returns>初始化后的目标。</returns>
		public static T EnsureInitialized<T> (ref T target) where T : class
		{
			return EnsureInitialized (ref target, GetDefaultCtorValue<T>);
		}
		
		/// <summary>
		/// 确保目标已初始化（使用指定工厂，Interlocked 快速路径）。
		/// </summary>
		/// <typeparam name="T">目标类型。</typeparam>
		/// <param name="target">目标引用（ref）。</param>
		/// <param name="initFunc">初始化工厂。</param>
		/// <returns>初始化后的目标。</returns>
		public static T EnsureInitialized<T> (ref T target, Func<T> initFunc) where T : class
		{
			if (target == null)
				Interlocked.CompareExchange (ref target, initFunc (), null);
			
			return target;
		}
		
		/// <summary>
		/// 确保目标已初始化（使用默认构造函数与锁/标志保护）。
		/// </summary>
		/// <typeparam name="T">目标类型。</typeparam>
		/// <param name="target">目标引用（ref）。</param>
		/// <param name="initialized">初始化标志（ref）。</param>
		/// <param name="syncRoot">同步锁对象（ref）。</param>
		/// <returns>初始化后的目标。</returns>
		public static T EnsureInitialized<T> (ref T target, ref bool initialized, ref object syncRoot)
		{
			return EnsureInitialized (ref target, ref initialized, ref syncRoot, GetDefaultCtorValue<T>);
		}
		
		/// <summary>
		/// 确保目标已初始化（使用指定工厂与锁/标志保护）。
		/// </summary>
		/// <typeparam name="T">目标类型。</typeparam>
		/// <param name="target">目标引用（ref）。</param>
		/// <param name="initialized">初始化标志（ref）。</param>
		/// <param name="syncRoot">同步锁对象（ref）。</param>
		/// <param name="initFunc">初始化工厂。</param>
		/// <returns>初始化后的目标。</returns>
		public static T EnsureInitialized<T> (ref T target, ref bool initialized, ref object syncRoot, Func<T> initFunc)
		{
			lock (syncRoot) {
				if (initialized)
					return target;
				
				initialized = true;
				return target = initFunc ();
			}
		}
		
		/// <summary>
		/// 使用类型的默认无参构造函数创建实例（内部使用）。
		/// </summary>
		/// <typeparam name="T">目标类型。</typeparam>
		/// <returns>创建出的实例。</returns>
		internal static T GetDefaultCtorValue<T> ()
		{
			try { 
				return Activator.CreateInstance<T> ();
			} catch { 
				throw new MissingMemberException ("The type being lazily initialized does not have a "
				                                  + "public, parameterless constructor.");
			}
		}
	}
}
//#endif

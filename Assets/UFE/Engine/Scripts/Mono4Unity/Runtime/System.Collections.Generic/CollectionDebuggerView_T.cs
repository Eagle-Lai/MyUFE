//
// CollectionDebuggerView.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.Diagnostics;

/// <summary>
/// 集合调试器视图（CollectionDebuggerView）。
/// <para>用途：从 Mono 移植的调试器类型代理——将泛型集合在调试器中以数组形式展示（Items 属性），</para>
/// <para>提供单类型与键值对两种视图。</para>
/// </summary>
namespace System.Collections.Generic
{
	//
	// Custom debugger type proxy to display collections as arrays
	//
	/// <summary>
	/// 单类型集合调试器视图：将 ICollection&lt;T&gt; 以数组形式呈现。
	/// </summary>
	/// <typeparam name="T">元素类型。</typeparam>
	internal sealed class CollectionDebuggerView<T>
	{
		/// <summary>目标集合。</summary>
		readonly ICollection<T> c;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="col">目标集合。</param>
		public CollectionDebuggerView (ICollection<T> col)
		{
			this.c = col;
		}
		
		/// <summary>
		/// 集合内容数组（调试器展开显示）。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items {
			get {
				var o = new T [c.Count];
				c.CopyTo (o, 0);
				return o;
			}
		}
	}

	/// <summary>
	/// 键值对集合调试器视图：将 ICollection&lt;KeyValuePair&lt;T,U&gt;&gt; 以数组形式呈现。
	/// </summary>
	/// <typeparam name="T">键类型。</typeparam>
	/// <typeparam name="U">值类型。</typeparam>
	internal sealed class CollectionDebuggerView<T, U>
	{
		/// <summary>目标集合。</summary>
		readonly ICollection<KeyValuePair<T, U>> c;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="col">目标集合。</param>
		public CollectionDebuggerView (ICollection<KeyValuePair<T, U>> col)
		{
			this.c = col;
		}

		/// <summary>
		/// 集合内容键值对数组（调试器展开显示）。
		/// </summary>
		[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
		public KeyValuePair<T, U>[] Items {
			get {
				var o = new KeyValuePair<T, U> [c.Count];
				c.CopyTo (o, 0);
				return o;
			}
		}
	}	
}
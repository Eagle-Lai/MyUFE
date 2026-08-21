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
/// 集合调试器视图（CollectionDebuggerView，非泛型）。
/// <para>用途：从 Mono 移植的调试器代理——将非泛型集合在调试器中以数组形式展示。</para>
/// </summary>
namespace System.Collections
{
	//
	// Custom debugger type proxy to display collections as arrays
	//
	/// <summary>
	/// 非泛型集合调试器视图：将 ICollection 以 object 数组形式呈现。
	/// </summary>
	internal sealed class CollectionDebuggerView
	{
		/// <summary>目标集合。</summary>
		readonly ICollection c;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="col">目标集合。</param>
		public CollectionDebuggerView (ICollection col)
		{
			this.c = col;
		}
		
		/// <summary>
		/// 集合内容数组（调试器展开显示）。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items {
			get {
				var o = new object [c.Count];
				c.CopyTo (o, 0);
				return o;
			}
		}
	}
}

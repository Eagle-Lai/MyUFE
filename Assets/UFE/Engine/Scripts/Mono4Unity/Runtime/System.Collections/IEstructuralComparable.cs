//
// IStructuralComparable.cs
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
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

//#if MOONLIGHT || NET_4_0 || MOBILE

using System;

/// <summary>
/// 结构性比较接口（IStructuralComparable）。
/// <para>用途：从 Mono 移植——支持按指定比较器对对象进行结构性比较（如元组/数组按元素比较）。</para>
/// </summary>
namespace System.Collections
{
	/// <summary>
	/// 结构性比较接口：使用指定比较器比较两个对象的结构。
	/// </summary>
	public interface IStructuralComparable {
		/// <summary>按指定比较器比较当前对象与另一个对象。</summary>
		/// <param name="other">待比较对象。</param>
		/// <param name="comparer">比较器。</param>
		/// <returns>比较结果（负/零/正）。</returns>
		int CompareTo (object other, IComparer comparer);
	}
}

//#endif
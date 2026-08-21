//
// IStructuralEquatable.cs
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
/// 结构性相等接口（IStructuralEquatable）。
/// <para>用途：从 Mono 移植——支持按指定比较器判断两个对象是否结构相等并计算结构哈希。</para>
/// </summary>
namespace System.Collections
{
	/// <summary>
	/// 结构性相等接口：支持按元素比较相等性与计算哈希。
	/// </summary>
	public interface IStructuralEquatable {
		/// <summary>按指定比较器判断当前对象与另一个对象是否结构相等。</summary>
		/// <param name="other">待比较对象。</param>
		/// <param name="comparer">比较器。</param>
		/// <returns>相等返回 true。</returns>
		bool Equals (object other, IEqualityComparer comparer);

		/// <summary>按指定比较器计算对象的结构哈希码。</summary>
		/// <param name="comparer">比较器。</param>
		/// <returns>哈希码。</returns>
		int GetHashCode (IEqualityComparer comparer);
	}
}

//#endif
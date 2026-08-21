//
// System.Collections.Generic.ISet.cs
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
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

//#if NET_4_0

/// <summary>
/// 集合接口（ISet&lt;T&gt;）。
/// <para>用途：从 Mono 移植——定义数学集合的集合运算（并/交/差/对称差）与子集/超集关系判断。</para>
/// </summary>
namespace System.Collections.Generic {
	/// <summary>
	/// 集合接口：提供集合代数运算。
	/// </summary>
	public interface ISet<T> : ICollection<T>
	{
		/// <summary>添加元素（已存在返回 false）。</summary>
		/// <param name="item">元素。</param>
		/// <returns>添加成功返回 true。</returns>
		new bool Add (T item);
		/// <summary>从当前集合移除 other 中的元素（差集）。</summary>
		/// <param name="other">另一集合。</param>
		void ExceptWith (IEnumerable<T> other);
		/// <summary>仅保留与 other 共有的元素（交集）。</summary>
		/// <param name="other">另一集合。</param>
		void IntersectWith (IEnumerable<T> other);
		/// <summary>是否是真子集。</summary>
		/// <param name="other">另一集合。</param>
		/// <returns>真子集返回 true。</returns>
		bool IsProperSubsetOf (IEnumerable<T> other);
		/// <summary>是否是真超集。</summary>
		/// <param name="other">另一集合。</param>
		/// <returns>真超集返回 true。</returns>
		bool IsProperSupersetOf (IEnumerable<T> other);
		/// <summary>是否是子集。</summary>
		/// <param name="other">另一集合。</param>
		/// <returns>子集返回 true。</returns>
		bool IsSubsetOf (IEnumerable<T> other);
		/// <summary>是否是超集。</summary>
		/// <param name="other">另一集合。</param>
		/// <returns>超集返回 true。</returns>
		bool IsSupersetOf (IEnumerable<T> other);
		/// <summary>是否与 other 有共同元素。</summary>
		/// <param name="other">另一集合。</param>
		/// <returns>有重叠返回 true。</returns>
		bool Overlaps (IEnumerable<T> other);
		/// <summary>判断两个集合是否相等（元素相同）。</summary>
		/// <param name="other">另一集合。</param>
		/// <returns>相等返回 true。</returns>
		bool SetEquals (IEnumerable<T> other);
		/// <summary>仅保留两集合中不同时存在的元素（对称差）。</summary>
		/// <param name="other">另一集合。</param>
		void SymmetricExceptWith (IEnumerable<T> other);
		/// <summary>合并 other 的所有元素（并集）。</summary>
		/// <param name="other">另一集合。</param>
		void UnionWith (IEnumerable<T> other);
	}
}
//#endif
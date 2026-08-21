// IConcurrentCollection.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

//#if NET_4_0 || MOBILE
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 生产者-消费者集合接口（IProducerConsumerCollection&lt;T&gt;）。
/// <para>用途：从 Mono 移植——定义线程安全的生产/消费集合统一接口（TryAdd 添加、TryTake 取出）。</para>
/// </summary>
namespace System.Collections.Concurrent
{
	/// <summary>
	/// 生产者-消费者集合接口：并发安全的添加/取出操作。
	/// </summary>
	public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection, IEnumerable
	{
		/// <summary>尝试添加元素。</summary>
		/// <param name="item">元素。</param>
		/// <returns>成功返回 true。</returns>
		bool TryAdd (T item);
		/// <summary>尝试取出一个元素。</summary>
		/// <param name="item">输出取出的元素。</param>
		/// <returns>成功返回 true。</returns>
		bool TryTake (out T item);
		/// <summary>复制到新数组。</summary>
		/// <returns>元素数组。</returns>
		T[] ToArray ();
		/// <summary>从指定索引开始复制到数组。</summary>
		/// <param name="array">目标数组。</param>
		/// <param name="index">起始索引。</param>
		void CopyTo (T[] array, int index);
	}
}
//#endif
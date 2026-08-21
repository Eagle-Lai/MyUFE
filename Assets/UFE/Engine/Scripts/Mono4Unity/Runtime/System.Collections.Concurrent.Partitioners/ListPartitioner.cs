// 
// ListPartitioner.cs
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

//#if NET_4_0 || MOBILE

using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// 列表分区器（ListPartitioner&lt;T&gt;）。
/// <para>用途：从 Mono 移植——将 IList&lt;T&gt; 按连续范围（Range）划分为指定数量的分区，</para>
/// <para>各分区互不重叠地覆盖源列表，供 Parallel 循环索引式并行消费。</para>
/// </summary>
namespace System.Collections.Concurrent.Partitioners
{
	// Represent a Range partitioner
	/// <summary>
	/// 范围分区器：按连续索引范围切分列表。
	/// </summary>
	internal class ListPartitioner<T> : OrderablePartitioner<T>
	{
		/// <summary>源列表。</summary>
		IList<T> source;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="source">源列表。</param>
		public ListPartitioner (IList<T> source) : base (true, true, true)
		{
			this.source = source;
		}

		/// <summary>
		/// 生成指定数量的可排序分区枚举器（按连续范围切分）。
		/// </summary>
		/// <param name="partitionCount">分区数量。</param>
		/// <returns>分区枚举器列表。</returns>
		public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");

			IEnumerator<KeyValuePair<long, T>>[] enumerators
				= new IEnumerator<KeyValuePair<long, T>>[partitionCount];

			int count = source.Count / partitionCount;
			int extra = 0;

			if (source.Count < partitionCount) {
				count = 1;
			} else {
				extra = source.Count % partitionCount;
				if (extra > 0)
					++count;
			}

			int currentIndex = 0;

			Range[] ranges = new Range[enumerators.Length];
			for (int i = 0; i < ranges.Length; i++) {
				ranges[i] = new Range (currentIndex,
				                       currentIndex + count);
				currentIndex += count;
				if (--extra == 0)
					--count;
			}

			for (int i = 0; i < enumerators.Length; i++) {
				enumerators[i] = GetEnumeratorForRange (ranges, i);
			}

			return enumerators;
		}

		/// <summary>
		/// 索引范围：起始位置与最后索引。
		/// </summary>
		class Range
		{
			/// <summary>当前实际索引（迭代推进）。</summary>
			public int Actual;
			/// <summary>最后索引（不含）。</summary>
			public readonly int LastIndex;

			/// <summary>
			/// 构造函数。
			/// </summary>
			/// <param name="frm">起始索引。</param>
			/// <param name="lastIndex">最后索引（不含）。</param>
			public Range (int frm, int lastIndex)
			{
				Actual = frm;
				LastIndex = lastIndex;
			}
		}

		/// <summary>
		/// 获取指定工作分区的枚举器（范围越界时返回空枚举器）。
		/// </summary>
		/// <param name="ranges">范围数组。</param>
		/// <param name="workerIndex">工作分区索引。</param>
		/// <returns>分区枚举器。</returns>
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRange (Range[] ranges, int workerIndex)
		{
			if (ranges[workerIndex].Actual >= source.Count)
			  return GetEmpty ();

			return GetEnumeratorForRangeInternal (ranges, workerIndex);
		}

		/// <summary>
		/// 空枚举器（不产生元素）。
		/// </summary>
		/// <returns>空枚举器。</returns>
		IEnumerator<KeyValuePair<long, T>> GetEmpty ()
		{
			yield break;
		}

		/// <summary>
		/// 范围迭代枚举器：从范围起始逐元素产出（含索引键）。
		/// </summary>
		/// <param name="ranges">范围数组。</param>
		/// <param name="workerIndex">工作分区索引。</param>
		/// <returns>分区枚举器。</returns>
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRangeInternal (Range[] ranges, int workerIndex)
		{
			Range range = ranges[workerIndex];
			int lastIndex = range.LastIndex;
			int index = range.Actual;

			for (int i = index; i < lastIndex; i = ++range.Actual) {
				yield return new KeyValuePair<long, T> (i, source[i]);
			}
		}
	}
}
//#endif

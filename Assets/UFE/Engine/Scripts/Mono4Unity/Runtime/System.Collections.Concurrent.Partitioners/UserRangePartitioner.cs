// 
// UserRangePartitioner.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2010 Jérémie "Garuma" Laval
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

/// <summary>
/// 用户范围分区器（UserRangePartitioner / UserLongRangePartitioner）。
/// <para>用途：从 Mono 移植——将 [start, end) 整数/长整型范围按固定区间大小（rangeSize）动态划分为分区，</para>
/// <para>供 Parallel.For 等按范围并行迭代使用。</para>
/// </summary>
namespace System.Collections.Concurrent.Partitioners
{
	/// <summary>
	/// 整数范围分区器：产出 (范围起始, 范围结束) 元组的可排序分区。
	/// </summary>
	internal class UserRangePartitioner : OrderablePartitioner<Tuple<int,  int>>
	{
		/// <summary>范围起始。</summary>
		readonly int start;
		/// <summary>范围结束（不含）。</summary>
		readonly int end;
		/// <summary>每个分区的区间大小。</summary>
		readonly int rangeSize;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="start">范围起始。</param>
		/// <param name="end">范围结束（不含）。</param>
		/// <param name="rangeSize">区间大小。</param>
		public UserRangePartitioner (int start, int end, int rangeSize) : base (true, true, true)
		{
			this.start = start;
			this.end = end;
			this.rangeSize = rangeSize;
		}

		/// <summary>
		/// 生成指定数量的分区枚举器（原子递增分配下一个区间）。
		/// </summary>
		/// <param name="partitionCount">分区数量。</param>
		/// <returns>分区枚举器列表。</returns>
		public override IList<IEnumerator<KeyValuePair<long, Tuple<int, int>>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");

			int currentIndex = 0;
			Func<int> getNextIndex = () => Interlocked.Increment (ref currentIndex) - 1;

			var enumerators = new IEnumerator<KeyValuePair<long, Tuple<int, int>>>[partitionCount];
			for (int i = 0; i < partitionCount; i++)
				enumerators[i] = GetEnumerator (getNextIndex);

			return enumerators;
		}

		/// <summary>
		/// 范围枚举器：依次取下一个区间（起始=index*rangeSize+start，结束=min(end, 起始+rangeSize)）。
		/// </summary>
		/// <param name="getNextIndex">获取下一个区间索引的委托。</param>
		/// <returns>分区枚举器。</returns>
		IEnumerator<KeyValuePair<long, Tuple<int, int>>> GetEnumerator (Func<int> getNextIndex)
		{
			while (true) {
				int index = getNextIndex ();
				int sliceStart = index * rangeSize + start;

				if (sliceStart >= end)
					break;

				yield return new KeyValuePair<long, Tuple<int, int>> (index, Tuple.Create (sliceStart, Math.Min (end, sliceStart + rangeSize)));
				sliceStart += rangeSize;
			}
		}
	}

	/// <summary>
	/// 长整型范围分区器：产出 (范围起始, 范围结束) 元组的可排序分区。
	/// </summary>
	internal class UserLongRangePartitioner : OrderablePartitioner<Tuple<long,  long>>
	{
		/// <summary>范围起始。</summary>
		readonly long start;
		/// <summary>范围结束（不含）。</summary>
		readonly long end;
		/// <summary>每个分区的区间大小。</summary>
		readonly long rangeSize;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="start">范围起始。</param>
		/// <param name="end">范围结束（不含）。</param>
		/// <param name="rangeSize">区间大小。</param>
		public UserLongRangePartitioner (long start, long end, long rangeSize) : base (true, true, true)
		{
			this.start = start;
			this.end = end;
			this.rangeSize = rangeSize;
		}

		/// <summary>
		/// 生成指定数量的分区枚举器。
		/// </summary>
		/// <param name="partitionCount">分区数量。</param>
		/// <returns>分区枚举器列表。</returns>
		public override IList<IEnumerator<KeyValuePair<long, Tuple<long, long>>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");

			long currentIndex = 0;
			Func<long> getNextIndex = () => Interlocked.Increment (ref currentIndex) - 1;

			var enumerators = new IEnumerator<KeyValuePair<long, Tuple<long, long>>>[partitionCount];
			for (int i = 0; i < partitionCount; i++)
				enumerators[i] = GetEnumerator (getNextIndex);

			return enumerators;
		}

		/// <summary>
		/// 范围枚举器：依次取下一个区间。
		/// </summary>
		/// <param name="getNextIndex">获取下一个区间索引的委托。</param>
		/// <returns>分区枚举器。</returns>
		IEnumerator<KeyValuePair<long, Tuple<long, long>>> GetEnumerator (Func<long> getNextIndex)
		{
			while (true) {
				long index = getNextIndex ();
				long sliceStart = index * rangeSize + start;

				if (sliceStart >= end)
					break;

				yield return new KeyValuePair<long, Tuple<long, long>> (index, Tuple.Create (sliceStart, Math.Min (end, sliceStart + rangeSize)));
				sliceStart += rangeSize;
			}
		}
	}
}
//#endif

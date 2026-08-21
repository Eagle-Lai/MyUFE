// 
// EnumerablePartitioner.cs
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

/// <summary>
/// 可枚举分区器（EnumerablePartitioner&lt;T&gt;）。
/// <para>用途：从 Mono 移植——将 IEnumerable&lt;T&gt; 按"块"（chunk）动态分区，块大小随迭代倍增，</para>
/// <para>供 Parallel 循环消费；简单模式（initialPartitionSize==multiplier==1）使用无列表的轻量枚举。</para>
/// </summary>
namespace System.Collections.Concurrent.Partitioners
{
	// Represent a chunk partitioner
	/// <summary>
	/// 块分区器：以递增块大小从可枚举源生成分区。
	/// </summary>
	internal class EnumerablePartitioner<T> : OrderablePartitioner<T>
	{
		/// <summary>数据源。</summary>
		IEnumerable<T> source;

		/// <summary>初始块大小。</summary>
		const int InitialPartitionSize = 1;
		/// <summary>块大小倍增因子。</summary>
		const int PartitionMultiplier = 2;

		/// <summary>初始块大小（实例字段）。</summary>
		int initialPartitionSize;
		/// <summary>块大小倍增因子（实例字段）。</summary>
		int partitionMultiplier;

		/// <summary>
		/// 构造函数（默认块参数）。
		/// </summary>
		/// <param name="source">数据源。</param>
		public EnumerablePartitioner (IEnumerable<T> source)
			: this (source, InitialPartitionSize, PartitionMultiplier)
		{

		}

		// This is used to get striped partitionning (for Take and Skip for instance
		/// <summary>
		/// 构造函数（指定块参数，用于条纹式分区如 Take/Skip）。
		/// </summary>
		/// <param name="source">数据源。</param>
		/// <param name="initialPartitionSize">初始块大小。</param>
		/// <param name="partitionMultiplier">块倍增因子。</param>
		public EnumerablePartitioner (IEnumerable<T> source, int initialPartitionSize, int partitionMultiplier)
			 : base (true, false, true)
		{
			this.source = source;
			this.initialPartitionSize = initialPartitionSize;
			this.partitionMultiplier = partitionMultiplier;
		}

		/// <summary>
		/// 生成指定数量的可排序分区枚举器。
		/// </summary>
		/// <param name="partitionCount">分区数量。</param>
		/// <returns>分区枚举器列表。</returns>
		public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");

			IEnumerator<KeyValuePair<long, T>>[] enumerators
				= new IEnumerator<KeyValuePair<long, T>>[partitionCount];

			PartitionerState state = new PartitionerState ();
			IEnumerator<T> src = source.GetEnumerator ();
			bool isSimple = initialPartitionSize == 1 && partitionMultiplier == 1;

			for (int i = 0; i < enumerators.Length; i++) {
				enumerators[i] = isSimple ? GetPartitionEnumeratorSimple (src, state, i == enumerators.Length - 1) : GetPartitionEnumerator (src, state);
			}

			return enumerators;
		}

		// This partitioner that is simpler than the general case (don't use a list) is called in the case
		// of initialPartitionSize == partitionMultiplier == 1
		/// <summary>
		/// 简单分区枚举器（无列表缓冲，逐个取元素）。
		/// </summary>
		/// <param name="src">数据源枚举器。</param>
		/// <param name="state">共享分区状态。</param>
		/// <param name="last">是否为最后一个分区（负责释放源）。</param>
		/// <returns>分区枚举器。</returns>
		IEnumerator<KeyValuePair<long, T>> GetPartitionEnumeratorSimple (IEnumerator<T> src,
		                                                                 PartitionerState state,
		                                                                 bool last)
		{
			long index = -1;
			var value = default (T);

			try {
				do {
					lock (state.SyncLock) {
						if (state.Finished)
							break;
						if (state.Finished = !src.MoveNext ())
							break;

						index = state.Index++;
						value = src.Current;
					}

					yield return new KeyValuePair<long, T> (index, value);
				} while (!state.Finished);
			} finally {
				if (last)
					src.Dispose ();
			}
		}

		/// <summary>
		/// 块分区枚举器：每次加锁抓取 count 个元素组成块，块大小随倍增因子增长。
		/// </summary>
		/// <param name="src">数据源枚举器。</param>
		/// <param name="state">共享分区状态。</param>
		/// <returns>分区枚举器。</returns>
		IEnumerator<KeyValuePair<long, T>> GetPartitionEnumerator (IEnumerator<T> src, PartitionerState state)
		{
			int count = initialPartitionSize;
			List<T> list = new List<T> ();

			while (!state.Finished) {
				list.Clear ();
				long ind = -1;

				lock (state.SyncLock) {
					if (state.Finished)
						break;

					ind = state.Index;

					for (int i = 0; i < count; i++) {
						if (state.Finished = !src.MoveNext ()) {
							if (list.Count == 0)
								yield break;
							else
								break;
						}

						list.Add (src.Current);
						state.Index++;
					}					
				}

				for (int i = 0; i < list.Count; i++)
					yield return new KeyValuePair<long, T> (ind + i, list[i]);

				count *= partitionMultiplier;
			}
		}

		/// <summary>
		/// 分区共享状态：索引与完成标志。
		/// </summary>
		class PartitionerState
		{
			/// <summary>是否已迭代完成。</summary>
			public bool Finished;
			/// <summary>全局元素索引。</summary>
			public long Index = 0;
			/// <summary>同步锁对象。</summary>
			public readonly object SyncLock = new object ();
		}
	}
}
//#endif

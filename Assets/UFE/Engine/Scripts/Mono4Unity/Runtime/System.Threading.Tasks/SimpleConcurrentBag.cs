// 
// SimpleConcurrentBag.cs
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

namespace System.Threading.Tasks
{

/// <summary>
/// 简单并发包（SimpleConcurrentBag&lt;T&gt;）。
/// <para>用途：从 Mono 移植的内部类——按线程索引划分环形双端队列槽位，</para>
/// <para>支持线程本地取出（TryTake）与跨线程偷取（TrySteal）的并发集合。</para>
/// </summary>
	/// <summary>
	/// 简单并发包内部类。
	/// </summary>
	internal class SimpleConcurrentBag<T>
	{
		/// <summary>各线程的双端队列槽位。</summary>
		readonly IConcurrentDeque<T>[] deques;
		/// <summary>是否单槽（无需偷取）。</summary>
		readonly bool unique;
		/// <summary>当前分配索引。</summary>
		int index = -1;
		
		/// <summary>偷取起始索引（线程局部）。</summary>
		[ThreadStatic]
		int stealIndex;

		public SimpleConcurrentBag (int num)
		{
			deques = new CyclicDeque<T>[num];
			for (int i = 0; i < deques.Length; i++) {
				deques[i] = new CyclicDeque<T> ();
			}
			unique = num <= 1;
		}

		public int GetNextIndex ()
		{
			return Interlocked.Increment (ref index);
		}

		public bool TryTake (int index, out T value)
		{
			value = default (T);

			return deques[index].PopBottom (out value) == PopResult.Succeed;
		}

		public bool TrySteal (int index, out T value)
		{
			value = default (T);

			if (unique)
				return false;

			const int roundThreshold = 3;

			for (int round = 0; round < roundThreshold; ++round) {
				if (stealIndex == index)
					stealIndex = (stealIndex + 1) % deques.Length;

				if (deques[(stealIndex = (stealIndex + 1) % deques.Length)].PopTop (out value) == PopResult.Succeed)
					return true;
			}

			return false;
		}

		public void Add (int index, T value)
		{
			deques[index].PushBottom (value);
		}
	}
}
//#endif
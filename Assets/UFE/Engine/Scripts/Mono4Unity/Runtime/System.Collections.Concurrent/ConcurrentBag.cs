// 
// ConcurrentBag.cs
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

//#if NET_4_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 并发包（ConcurrentBag&lt;T&gt;）。
/// <para>用途：从 Mono 移植的线程安全无序集合——按线程 ID 划分环形双端队列（CyclicDeque）槽位，</para>
/// <para>支持线程本地快速添加/取出与跨线程偷取（work-stealing），适合无序的生产者-消费者。</para>
/// </summary>
namespace System.Collections.Concurrent
{
	/// <summary>
	/// 并发包：线程安全的无序元素集合。
	/// </summary>
	[ComVisible (false)]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<>))]
	public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable
	{
		/// <summary>启用"添加提示"缓存优化的槽位数量阈值。</summary>
		const int hintThreshold = 20;
		
		/// <summary>元素数量。</summary>
		int count;
		
		// We only use the add hints when number of slot is above hintThreshold
		// so to not waste memory space and the CAS overhead
		/// <summary>添加提示队列（跨线程偷取用，仅槽位数超阈值时启用）。</summary>
		ConcurrentQueue<int> addHints = new ConcurrentQueue<int> ();
		
		/// <summary>按线程 ID 映射到各线程的双端队列槽位。</summary>
		ConcurrentDictionary<int, CyclicDeque<T>> container = new ConcurrentDictionary<int, CyclicDeque<T>> ();
		
		public ConcurrentBag ()
		{
		}
		
		public ConcurrentBag (IEnumerable<T> enumerable) : this ()
		{
			foreach (T item in enumerable)
				Add (item);
		}
		
		public void Add (T item)
		{
			int index;
			CyclicDeque<T> bag = GetBag (out index);
			bag.PushBottom (item);
			
			// Cache operation ?
			if (container.Count > hintThreshold)
				addHints.Enqueue (index);
			
			Interlocked.Increment (ref count);
		}
		
		bool IProducerConsumerCollection<T>.TryAdd (T element)
		{
			Add (element);
			return true;
		}
		
		public bool TryTake (out T item)
		{
			item = default (T);
			
			if (count == 0)
				return false;
			
			int hintIndex;
			CyclicDeque<T> bag = GetBag (out hintIndex);
			bool hintEnabled = container.Count > hintThreshold;
			
			if (bag == null || bag.PopBottom (out item) != PopResult.Succeed) {
				foreach (var other in container) {
					// Try to retrieve something based on a hint
					bool result = hintEnabled && addHints.TryDequeue (out hintIndex) && container[hintIndex].PopTop (out item) == PopResult.Succeed;
					
					// We fall back to testing our slot
					if (!result && other.Value != bag)
						result = other.Value.PopTop (out item) == PopResult.Succeed;
					
					// If we found something, stop
					if (result) {
						Interlocked.Decrement (ref count);
						return true;
					}
				}
			} else {
				Interlocked.Decrement (ref count);
				return true;
			}
			
			return false;
		}
		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsEmpty {
			get {
				return count == 0;
			}
		}
		
		object System.Collections.ICollection.SyncRoot  {
			get {
				return this;
			}
		}
		
		bool System.Collections.ICollection.IsSynchronized  {
			get {
				return true;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}
		
		public IEnumerator<T> GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}
		
		IEnumerator<T> GetEnumeratorInternal ()
		{
			foreach (var bag in container)
				foreach (T item in bag.Value.GetEnumerable ())
					yield return item;
		}
		
		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			T[] a = array as T[];
			if (a == null)
				return;
			
			CopyTo (a, index);
		}
		
		public void CopyTo (T[] array, int index)
		{
			int c = count;
			if (array.Length < c + index)
				throw new InvalidOperationException ("Array is not big enough");
			
			CopyTo (array, index, c);
		}
		
		void CopyTo (T[] array, int index, int num)
		{
			int i = index;
			
			foreach (T item in this) {
				if (i >= num)
					break;
				
				array[i++] = item;
			}
		}
		
		public T[] ToArray ()
		{
			int c = count;
			T[] temp = new T[c];
			
			CopyTo (temp, 0, c);
			
			return temp;
		}
		
		int GetIndex ()
		{
			return Thread.CurrentThread.ManagedThreadId;
		}
		
		CyclicDeque<T> GetBag (out int index)
		{
			index = GetIndex ();
			CyclicDeque<T> value;
			if (container.TryGetValue (index, out value))
				return value;
			
			return container.GetOrAdd (index, new CyclicDeque<T> ());
		}
	}
}
//#endif
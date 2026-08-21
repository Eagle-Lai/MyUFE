//
// TaskCompletionQueue.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

/// <summary>
/// 任务完成队列（TaskCompletionQueue&lt;TCompletion&gt;）。
/// <para>用途：从 Mono 移植的内部结构——用于存储任务的延续/完成回调集合，</para>
/// <para>采用"单元素快速路径 + 并发有序列表"的组合，支持并发添加/移除/取出。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务完成回调队列结构。
	/// </summary>
	internal struct TaskCompletionQueue<TCompletion> where TCompletion : class
	{
		/// <summary>单元素快速槽位。</summary>
		TCompletion single;
		/// <summary>多元素并发有序列表。</summary>
		ConcurrentOrderedList<TCompletion> completed;

		public void Add (TCompletion continuation)
		{
			if (single == null && Interlocked.CompareExchange (ref single, continuation, null) == null)
				return;
			if (completed == null)
				Interlocked.CompareExchange (ref completed, new ConcurrentOrderedList<TCompletion> (), null);
			completed.TryAdd (continuation);
		}

		public bool Remove (TCompletion continuation)
		{
			TCompletion temp = single;
			if (temp != null && temp == continuation && Interlocked.CompareExchange (ref single, null, continuation) == continuation)
				return true;
			if (completed != null)
				return completed.TryRemove (continuation);
			return false;
		}

		public bool HasElements {
			get {
				return single != null || (completed != null && completed.Count != 0);
			}
		}

		public bool TryGetNextCompletion (out TCompletion continuation)
		{
			continuation = null;

			if (single != null && (continuation = Interlocked.Exchange (ref single, null)) != null)
				return true;

			return completed != null && completed.TryPop (out continuation);
		}
	}
}

//#endif
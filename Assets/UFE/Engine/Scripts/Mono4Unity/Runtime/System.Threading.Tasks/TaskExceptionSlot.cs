//
// TaskExceptionSlot.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
using System.Collections.Concurrent;

/// <summary>
/// 任务异常槽（TaskExceptionSlot）。
/// <para>用途：从 Mono 移植的内部类——保存任务的聚合异常及是否已被观察，</para>
/// <para>在终结器中触发未观察异常事件（未观察异常在 .NET 4.5 后默认不再使进程崩溃）。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务异常槽内部类。
	/// </summary>
	internal class TaskExceptionSlot
	{
		/// <summary>任务聚合异常。</summary>
		public volatile AggregateException  Exception;
		/// <summary>异常是否已被观察。</summary>
		public volatile bool                Observed;
		/// <summary>子任务异常队列。</summary>
		public ConcurrentQueue<AggregateException> ChildExceptions;

		/// <summary>父任务引用。</summary>
		Task parent;

		public TaskExceptionSlot (Task parent)
		{
			this.parent = parent;
		}

		~TaskExceptionSlot ()
		{
			if (Exception != null && !Observed && !TaskScheduler.FireUnobservedEvent (parent, Exception).Observed) {
				// NET 4.5 changed the default exception behavior for unobserved exceptions. Unobserved exceptions still cause
				// the UnobservedTaskException event to be raised but the process will not crash by default
				//
				// .NET allows to configure this using config element ThrowUnobservedTaskExceptions
				//
#if !NET_4_5
				throw Exception;
#endif
			}
		}
	}
}

//#endif
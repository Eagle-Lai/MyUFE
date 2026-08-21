// TaskSchedulerException.cs
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
using System.Runtime.Serialization;

/// <summary>
/// 任务调度器异常（TaskSchedulerException）。
/// <para>用途：从 Mono 移植——表示任务调度器（TaskScheduler）执行时抛出的异常。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务调度器异常类。
	/// </summary>
	public class TaskSchedulerException : Exception
	{
		/// <summary>默认错误消息。</summary>
		const string exceptionDefaultMessage = "An exception was thrown by a TaskScheduler";

		public TaskSchedulerException () : base (exceptionDefaultMessage)
		{

		}

		public TaskSchedulerException (string message) : base (message)
		{

		}

		protected TaskSchedulerException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{

		}

		public TaskSchedulerException (Exception innerException)
			: base (exceptionDefaultMessage, innerException)
		{

		}

		public TaskSchedulerException (string message, Exception innerException)
			: base (message, innerException)
		{

		}
	}
}
//#endif
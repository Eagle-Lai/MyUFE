// IScheduler.cs
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
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// 任务调度器接口（IScheduler）。
/// <para>用途：从 Mono 移植的内部接口——任务调度与参与者等待（工作窃取调度器核心）。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务调度器接口。
	/// </summary>
	internal interface IScheduler: IDisposable
	{
		/// <summary>添加任务到调度队列。</summary>
		/// <param name="t">任务。</param>
		void AddWork (Task t);
		/// <summary>参与执行工作直到指定任务完成。</summary>
		/// <param name="task">目标任务。</param>
		void ParticipateUntil (Task task);
		/// <summary>参与执行工作直到任务完成或超时。</summary>
		/// <param name="task">目标任务。</param>
		/// <param name="predicateEvt">谓词事件。</param>
		/// <param name="millisecondsTimeout">超时毫秒。</param>
		/// <returns>是否按时完成。</returns>
		bool ParticipateUntil (Task task, ManualResetEventSlim predicateEvt, int millisecondsTimeout);
		/// <summary>唤醒所有等待参与者。</summary>
		void PulseAll ();
	}
}
//#endif
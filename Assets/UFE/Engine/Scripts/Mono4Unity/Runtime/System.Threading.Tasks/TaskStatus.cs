// 
// TaskStatus.cs
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

/// <summary>
/// 任务状态（TaskStatus）。
/// <para>用途：从 Mono 移植——描述 Task 生命周期中的当前状态。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务状态枚举。
	/// </summary>
	public enum TaskStatus
	{
		/// <summary>任务已创建但尚未调度。</summary>
		Created,
		/// <summary>任务已创建并等待激活。</summary>
		WaitingForActivation,
		/// <summary>任务已调度等待运行。</summary>
		WaitingToRun,
		/// <summary>任务正在运行。</summary>
		Running,
		/// <summary>任务已完成但等待子任务完成。</summary>
		WaitingForChildrenToComplete,
		/// <summary>任务成功运行到完成。</summary>
		RanToCompletion,
		/// <summary>任务被取消。</summary>
		Canceled,
		/// <summary>任务执行出错。</summary>
		Faulted
	}
}
//#endif
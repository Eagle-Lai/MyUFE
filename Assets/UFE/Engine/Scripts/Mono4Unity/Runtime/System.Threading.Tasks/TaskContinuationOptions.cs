// TaskContinuationKind.cs
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

/// <summary>
/// 任务延续选项（TaskContinuationOptions）。
/// <para>用途：从 Mono 移植——控制延续任务（ContinueWith）的调度时机与执行条件。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务延续选项枚举（Flags）。
	/// </summary>
	[System.FlagsAttribute, System.SerializableAttribute]
	public enum TaskContinuationOptions
	{
		/// <summary>默认选项。</summary>
		None                  = 0x00000,
		/// <summary>优先公平调度。</summary>
		PreferFairness        = 0x00001,
		/// <summary>长时间运行任务。</summary>
		LongRunning           = 0x00002,
		/// <summary>附加到父任务。</summary>
		AttachedToParent      = 0x00004,
#if NET_4_5
		/// <summary>拒绝子任务附加。</summary>
		DenyChildAttach       = 0x00008,
		/// <summary>隐藏调度器。</summary>
		HideScheduler         = 0x00010,
		/// <summary>延迟取消。</summary>
		LazyCancellation      = 0x00020,
#endif
		/// <summary>前置任务成功完成时不执行延续。</summary>
		NotOnRanToCompletion  = 0x10000,
		/// <summary>前置任务出错时不执行延续。</summary>
		NotOnFaulted          = 0x20000,
		/// <summary>前置任务被取消时不执行延续。</summary>
		NotOnCanceled         = 0x40000,
		/// <summary>仅当前置任务成功完成时执行延续。</summary>
		OnlyOnRanToCompletion = 0x60000,
		/// <summary>仅当前置任务出错时执行延续。</summary>
		OnlyOnFaulted         = 0x50000,
		/// <summary>仅当前置任务被取消时执行延续。</summary>
		OnlyOnCanceled        = 0x30000,
		/// <summary>同步执行延续（不排队）。</summary>
		ExecuteSynchronously  = 0x80000,
	}
}
//#endif
// ParallelState.cs
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

/// <summary>
/// 并行循环状态（ParallelLoopState）。
/// <para>用途：从 Mono 移植——允许并行循环迭代内调用 Stop/Break 提前终止循环，并查询循环状态。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 并行循环状态：可控制循环的停止/中断。
	/// </summary>
	[System.Diagnostics.DebuggerDisplayAttribute ("ShouldExitCurrentIteration = {ShouldExitCurrentIteration}")]
	public class ParallelLoopState
	{
		/// <summary>
		/// 循环外部共享信息（跨迭代共享的循环状态）。
		/// </summary>
		internal class ExternalInfos
		{
			/// <summary>是否已调用 Stop。</summary>
			public bool IsStopped;
			/// <summary>是否已调用 Break（原子标志）。</summary>
			public AtomicBooleanValue IsBroken = new AtomicBooleanValue ();
			/// <summary>是否发生异常。</summary>
			public volatile bool IsExceptional;
			/// <summary>最低中断迭代号。</summary>
			public long? LowestBreakIteration;
		}

		/// <summary>外部共享信息引用。</summary>
		ExternalInfos extInfos;

		/// <summary>
		/// 内部构造函数。
		/// </summary>
		/// <param name="extInfos">外部共享信息。</param>
		internal ParallelLoopState (ExternalInfos extInfos)
		{
			this.extInfos = extInfos;
		}

		public bool IsStopped {
			get {
				return extInfos.IsStopped;
			}
		}

		public bool IsExceptional {
			get {
				return extInfos.IsExceptional;
			}
		}

		public long? LowestBreakIteration {
			get {
				return extInfos.LowestBreakIteration;
			}
		}

		internal int CurrentIteration {
			get;
			set;
		}

		public bool ShouldExitCurrentIteration {
			get {
				return IsExceptional || IsStopped;
			}
		}

		public void Break ()
		{
			if (extInfos.IsStopped)
				throw new InvalidOperationException ("The Stop method was previously called. Break and Stop may not be used in combination by iterations of the same loop.");

			bool result = extInfos.IsBroken.Exchange (true);
			if (!result)
				extInfos.LowestBreakIteration = CurrentIteration;
		}

		public void Stop ()
		{
			if (extInfos.IsBroken.Value)
				throw new InvalidOperationException ("The Break method was previously called. Break and Stop may not be used in combination by iterations of the same loop.");
			extInfos.IsStopped = true;
		}
	}

}
//#endif
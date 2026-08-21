// 
// ParallelLoopResult.cs
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
/// 并行循环结果（ParallelLoopResult）。
/// <para>用途：从 Mono 移植——描述 Parallel 循环的执行结果（是否完成与最低中断迭代）。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 并行循环结果结构体。
	/// </summary>
	public struct ParallelLoopResult
	{
		/// <summary>
		/// 内部构造函数。
		/// </summary>
		/// <param name="lowest">最低中断迭代。</param>
		/// <param name="isCompleted">是否完成。</param>
		internal ParallelLoopResult (long? lowest, bool isCompleted) : this ()
		{
			LowestBreakIteration = lowest;
			IsCompleted = isCompleted;
		}

		/// <summary>最低的中断（Break）迭代号；未中断为 null。</summary>
		public long? LowestBreakIteration {
			get;
			private set;
		}

		/// <summary>循环是否完整执行完成（未被 Stop/Break 中断）。</summary>
		public bool IsCompleted {
			get;
			private set;
		}
	}
}
//#endif
// SpinWait.cs
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


//#if NET_4_0 || MOBILE
using System;

/// <summary>
/// 自旋等待（SpinWait）。
/// <para>用途：从 Mono 移植的忙等待辅助结构——在等待条件时先短时自旋（CPU 忙等），</para>
/// <para>周期性让出 CPU（Thread.Sleep(0)），在多核/单核机器上自适应，避免过度占用 CPU。</para>
/// </summary>
namespace System.Threading
{
	/// <summary>
	/// 自旋等待结构：提供单次自旋与按条件/超时等待。
	/// </summary>
	public struct SpinWait
	{
		// The number of step until SpinOnce yield on multicore machine
		/// <summary>多核机器上自旋多少步后让出 CPU。</summary>
		const           int  step = 10;
		/// <summary>单次自旋的最大时间（步数）。</summary>
		const           int  maxTime = 200;
		/// <summary>是否为单核机器。</summary>
		static readonly bool isSingleCpu = (Environment.ProcessorCount == 1);

		/// <summary>当前自旋次数。</summary>
		int ntime;

		/// <summary>
		/// 单次自旋：单核直接让出 CPU；多核按步数周期让出，其余时间忙等。
		/// </summary>
		public void SpinOnce ()
		{
			ntime += 1;

			if (isSingleCpu) {
				// On a single-CPU system, spinning does no good
				//FIXME: Thread.Yield ();
				Thread.Sleep(0);
			} else {
				if (ntime % step == 0)
					//FIXME: Thread.Yield ();
					Thread.Sleep(0);
				else
					// Multi-CPU system might be hyper-threaded, let other thread run
					Thread.SpinWait (Math.Min (ntime, maxTime) << 1);
			}
		}

		/// <summary>
		/// 自旋直到条件满足。
		/// </summary>
		/// <param name="condition">等待条件。</param>
		public static void SpinUntil (Func<bool> condition)
		{
			SpinWait sw = new SpinWait ();
			while (!condition ())
				sw.SpinOnce ();
		}

		/// <summary>
		/// 自旋直到条件满足或超时。
		/// </summary>
		/// <param name="condition">等待条件。</param>
		/// <param name="timeout">超时时间。</param>
		/// <returns>条件满足返回 true；超时返回 false。</returns>
		public static bool SpinUntil (Func<bool> condition, TimeSpan timeout)
		{
			return SpinUntil (condition, (int)timeout.TotalMilliseconds);
		}

		/// <summary>
		/// 自旋直到条件满足或超时（毫秒）。
		/// </summary>
		/// <param name="condition">等待条件。</param>
		/// <param name="millisecondsTimeout">超时毫秒数。</param>
		/// <returns>条件满足返回 true；超时返回 false。</returns>
		public static bool SpinUntil (Func<bool> condition, int millisecondsTimeout)
		{
			SpinWait sw = new SpinWait ();
			Watch watch = Watch.StartNew ();

			while (!condition ()) {
				if (watch.ElapsedMilliseconds > millisecondsTimeout)
					return false;
				sw.SpinOnce ();
			}

			return true;
		}

		/// <summary>
		/// 重置自旋计数。
		/// </summary>
		public void Reset ()
		{
			ntime = 0;
		}

		/// <summary>
		/// 下一次自旋是否会让出 CPU（单核恒为 true）。
		/// </summary>
		public bool NextSpinWillYield {
			get {
				return isSingleCpu ? true : ntime % step == 0;
			}
		}

		/// <summary>
		/// 当前自旋次数。
		/// </summary>
		public int Count {
			get {
				return ntime;
			}
		}
	}
}
//#endif

// Task.cs
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
/// 计时器（Watch）。
/// <para>用途：从 Mono 移植的轻量毫秒计时器（内部结构），用于 SpinWait 等处的超时测量。</para>
/// </summary>
namespace System.Threading
{
	/// <summary>
	/// 内部毫秒计时器结构。
	/// </summary>
	internal struct Watch
	{
		/// <summary>起始刻度（Tick）。</summary>
		long startTicks;

		/// <summary>
		/// 创建并启动新计时器。
		/// </summary>
		/// <returns>已启动的计时器。</returns>
		public static Watch StartNew ()
		{
			Watch watch = new Watch ();
			watch.Start ();
			return watch;
		}

		/// <summary>
		/// 启动计时。
		/// </summary>
		public void Start ()
		{
			startTicks = TicksNow ();
		}

		/// <summary>
		/// 停止计时（空实现，仅供 API 兼容）。
		/// </summary>
		public void Stop ()
		{

		}

		/// <summary>
		/// 从启动起经过的毫秒数。
		/// </summary>
		public long ElapsedMilliseconds {
			get {
				return (TicksNow () - startTicks) / TimeSpan.TicksPerMillisecond;
			}
		}

		/// <summary>
		/// 获取当前单调时间刻度（FIXME：当前使用 DateTime.Now 易受系统时间修改影响）。
		/// </summary>
		/// <returns>当前刻度。</returns>
		static long TicksNow ()
		{
			//FIXME: return DateTime.GetTimeMonotonic (); 
			return DateTime.Now.Ticks;	// <--- problems if user changes the hour or date of the system
		}
	}
}
//#endif

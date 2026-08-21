//
// TaskConstants_T.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2011 Xamarin Inc.
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

//#if NET_4_5 || MOBILE
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// 泛型任务常量（TaskConstants&lt;T&gt;）。
/// <para>用途：从 Mono 移植的内部常量——提供泛型已取消任务（Canceled）单例。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 泛型任务常量内部类。
	/// </summary>
	internal class TaskConstants<T>
	{
		/// <summary>已取消的泛型任务。</summary>
		internal static readonly Task<T> Canceled;

		static TaskConstants ()
		{
			var tcs = new TaskCompletionSource<T> ();
			tcs.SetCanceled ();
			Canceled = tcs.Task;
		}
	}
}

//#endif
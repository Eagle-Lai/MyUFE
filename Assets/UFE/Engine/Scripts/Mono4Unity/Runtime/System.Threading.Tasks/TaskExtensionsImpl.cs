//
// TaskExtensionsImpl.cs
//
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
// Copyright (C) 2013 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//#if NET_4_0 || MOBILE

/// <summary>
/// 任务扩展实现（TaskExtensionsImpl）。
/// <para>用途：从 Mono 移植的内部实现——为嵌套任务（Task&lt;Task&lt;T&gt;&gt;）提供 Unwrap 展开，</para>
/// <para>将嵌套任务的结果/异常/取消透传到外层 TaskCompletionSource。</para>
/// </summary>
namespace System.Threading.Tasks 
{
	/// <summary>
	/// 任务扩展实现静态类。
	/// </summary>
	static class TaskExtensionsImpl
	{
		/// <summary>延续选项（同步执行）。</summary>
		const TaskContinuationOptions options = TaskContinuationOptions.ExecuteSynchronously;

		public static Task<TResult> Unwrap<TResult> (Task<Task<TResult>> task)
		{
			var src = new TaskCompletionSource<TResult> ();
			task.ContinueWith ((t, arg) => Cont (t, (TaskCompletionSource<TResult>) arg), src, CancellationToken.None, options, TaskScheduler.Current);

			return src.Task;
		}

		public static Task Unwrap (Task<Task> task)
		{
			var src = new TaskCompletionSource<object> ();
			task.ContinueWith ((t, arg) => Cont (t, (TaskCompletionSource<object>) arg), src, CancellationToken.None, options, TaskScheduler.Current);

			return src.Task;
		}

		static void SetResult (Task source, TaskCompletionSource<object> dest)
		{
			if (source.IsCanceled)
				dest.SetCanceled ();
			else if (source.IsFaulted)
				dest.SetException (source.Exception.InnerExceptions);
			else
				dest.SetResult (null);
		}

		static void Cont (Task<Task> source, TaskCompletionSource<object> dest)
		{
			if (source.IsCanceled)
				dest.SetCanceled ();
			else if (source.IsFaulted)
				dest.SetException (source.Exception.InnerExceptions);
			else
				source.Result.ContinueWith ((t, arg) => SetResult (t, (TaskCompletionSource<object>) arg), dest, CancellationToken.None, options, TaskScheduler.Current);
		}

		static void SetResult<TResult> (Task<TResult> source, TaskCompletionSource<TResult> dest)
		{
			if (source.IsCanceled)
				dest.SetCanceled ();
			else if (source.IsFaulted)
				dest.SetException (source.Exception.InnerExceptions);
			else
				dest.SetResult (source.Result);
		}

		static void Cont<TResult> (Task<Task<TResult>> source, TaskCompletionSource<TResult> dest)
		{
			if (source.IsCanceled)
				dest.SetCanceled ();
			else if (source.IsFaulted)
				dest.SetException (source.Exception.InnerExceptions);
			else
				source.Result.ContinueWith ((t, arg) => SetResult (t, (TaskCompletionSource<TResult>) arg), dest, CancellationToken.None, options, TaskScheduler.Current);
		}
	}
}

//#endif
// AggregateException.cs
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// 聚合异常（AggregateException）。
/// <para>用途：从 Mono 移植的并行任务异常容器——将多个同时发生的异常聚合为一个异常抛出，</para>
/// <para>支持展平（Flatten）嵌套聚合异常、按谓词处理（Handle）内部异常。</para>
/// </summary>
namespace System
{

	/// <summary>
	/// 聚合异常：包含多个内部异常（InnerExceptions）。
	/// </summary>
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerDisplay ("Count = {InnerExceptions.Count}")]
	public class AggregateException : Exception
	{
		/// <summary>内部异常列表。</summary>
		List<Exception> innerExceptions = new List<Exception> ();
		/// <summary>默认错误消息。</summary>
		const string defaultMessage = "One or more errors occured";

		/// <summary>
		/// 默认构造函数。
		/// </summary>
		public AggregateException () : base (defaultMessage)
		{
		}

		/// <summary>
		/// 构造函数（指定消息）。
		/// </summary>
		/// <param name="message">错误消息。</param>
		public AggregateException (string message): base (message)
		{
		}

		/// <summary>
		/// 构造函数（指定消息与单个内部异常）。
		/// </summary>
		/// <param name="message">错误消息。</param>
		/// <param name="innerException">内部异常。</param>
		public AggregateException (string message, Exception innerException): base (message, innerException)
		{
			if (innerException == null)
				throw new ArgumentNullException ("innerException");
			innerExceptions.Add (innerException);
		}
		/*
		protected AggregateException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		*/

		/// <summary>
		/// 构造函数（从异常数组）。
		/// </summary>
		/// <param name="innerExceptions">内部异常数组。</param>
		public AggregateException (params Exception[] innerExceptions)
			: this (string.Empty, innerExceptions)
		{
		}

		/// <summary>
		/// 构造函数（指定消息与异常数组）。
		/// </summary>
		/// <param name="message">错误消息。</param>
		/// <param name="innerExceptions">内部异常数组。</param>
		public AggregateException (string message, params Exception[] innerExceptions)
			: base (message, innerExceptions == null || innerExceptions.Length == 0 ? null : innerExceptions[0])
		{
			if (innerExceptions == null)
				throw new ArgumentNullException ("innerExceptions");
			foreach (var exception in innerExceptions)
				if (exception == null)
					throw new ArgumentException ("One of the inner exception is null", "innerExceptions");

			this.innerExceptions.AddRange (innerExceptions);
		}

		/// <summary>
		/// 构造函数（从异常集合）。
		/// </summary>
		/// <param name="innerExceptions">内部异常集合。</param>
		public AggregateException (IEnumerable<Exception> innerExceptions)
			: this (defaultMessage, innerExceptions)
		{
		}

		/// <summary>
		/// 构造函数（指定消息与异常集合）。
		/// </summary>
		/// <param name="message">错误消息。</param>
		/// <param name="innerExceptions">内部异常集合。</param>
		public AggregateException (string message, IEnumerable<Exception> innerExceptions)
			: this (message, new List<Exception> (innerExceptions).ToArray ())
		{
		}

		/// <summary>
		/// 展平聚合异常：递归展开所有嵌套的 AggregateException，返回只含叶异常的新聚合异常。
		/// </summary>
		/// <returns>展平后的聚合异常。</returns>
		public AggregateException Flatten ()
		{
			List<Exception> inner = new List<Exception> ();

			foreach (Exception e in innerExceptions) {
				AggregateException aggEx = e as AggregateException;
				if (aggEx != null) {
					inner.AddRange (aggEx.Flatten ().InnerExceptions);
				} else {
					inner.Add (e);
				}				
			}

			return new AggregateException (inner);
		}

		/// <summary>
		/// 用谓词处理内部异常：谓词返回 false 或抛异常的异常将被重新聚合并抛出。
		/// </summary>
		/// <param name="predicate">处理谓词（返回 true 表示已处理）。</param>
		public void Handle (Func<Exception, bool> predicate)
		{
			List<Exception> failed = new List<Exception> ();
			foreach (var e in innerExceptions) {
				try {
					if (!predicate (e))
						failed.Add (e);
				} catch {
					throw new AggregateException (failed);
				}
			}
			if (failed.Count > 0)
				throw new AggregateException (failed);
		}

		/// <summary>
		/// 内部异常只读集合。
		/// </summary>
		public ReadOnlyCollection<Exception> InnerExceptions {
			get {
				return innerExceptions.AsReadOnly ();
			}
		}

		/// <summary>
		/// 添加子聚合异常（内部使用）。
		/// </summary>
		/// <param name="childEx">子聚合异常。</param>
		internal void AddChildException (AggregateException childEx)
		{
			if (innerExceptions == null)
				innerExceptions = new List<Exception> ();
			if (childEx == null)
				return;

			innerExceptions.Add (childEx);
		}

		/// <summary>
		/// 生成调试字符串（含每个内部异常的完整信息）。
		/// </summary>
		/// <returns>调试字符串。</returns>
		public override string ToString ()
		{
			System.Text.StringBuilder finalMessage = new System.Text.StringBuilder (base.ToString ());

			int currentIndex = -1;
			foreach (Exception e in innerExceptions) {
				finalMessage.Append (Environment.NewLine);
				finalMessage.Append (" --> (Inner exception ");
				finalMessage.Append (++currentIndex);
				finalMessage.Append (") ");
				finalMessage.Append (e.ToString ());
				finalMessage.Append (Environment.NewLine);
			}
			return finalMessage.ToString ();
		}
		/*
		public override void GetObjectData (SerializationInfo info,	StreamingContext context)
		{
			if (info == null) {
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue ("InnerExceptions", innerExceptions.ToArray(), typeof (Exception[]));
		}
		*/
		/// <summary>
		/// 获取最底层异常（若无内部异常则返回自身）。
		/// </summary>
		/// <returns>根异常。</returns>
		public override Exception GetBaseException ()
		{
			if (innerExceptions == null || innerExceptions.Count == 0)
				return this;
			return innerExceptions[0].GetBaseException ();
		}
	}
}
//#endif

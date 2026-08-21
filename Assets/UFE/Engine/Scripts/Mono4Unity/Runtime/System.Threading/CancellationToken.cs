//
// CancellationToken.cs
//
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2009 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Threading;
using System.Diagnostics;

/// <summary>
/// 取消令牌（CancellationToken）。
/// <para>用途：从 Mono 移植的协作式取消机制——包装 CancellationTokenSource，</para>
/// <para>用于向任务/操作传递取消请求，支持注册取消回调、检查取消状态与抛出取消异常。</para>
/// </summary>
namespace System.Threading
{
	/// <summary>
	/// 取消令牌结构：可与取消源绑定，用于检查取消请求与注册回调。
	/// </summary>
	[DebuggerDisplay ("IsCancellationRequested = {IsCancellationRequested}")]
	public struct CancellationToken
	{
		/// <summary>关联的取消源（null 表示不可取消）。</summary>
		readonly CancellationTokenSource source;

		/// <summary>
		/// 构造函数（按是否已取消创建）。
		/// </summary>
		/// <param name="canceled">是否已取消。</param>
		public CancellationToken (bool canceled)
			: this (canceled ? CancellationTokenSource.CanceledSource : null)
		{
		}

		/// <summary>
		/// 内部构造函数（绑定取消源）。
		/// </summary>
		/// <param name="source">取消源。</param>
		internal CancellationToken (CancellationTokenSource source)
		{
			this.source = source;
		}

		/// <summary>
		/// 空取消令牌（不可取消）。
		/// </summary>
		public static CancellationToken None {
			get {
				// simply return new struct value, it's the fastest option
				// and we don't have to bother with reseting source
				return new CancellationToken ();
			}
		}

		/// <summary>
		/// 注册取消回调（不捕获同步上下文）。
		/// </summary>
		/// <param name="callback">取消回调。</param>
		/// <returns>注册对象（可 Dispose 取消注册）。</returns>
		public CancellationTokenRegistration Register (Action callback)
		{
			return Register (callback, false);
		}

		/// <summary>
		/// 注册取消回调（可选捕获同步上下文）。
		/// </summary>
		/// <param name="callback">取消回调。</param>
		/// <param name="useSynchronizationContext">是否在同步上下文上执行回调。</param>
		/// <returns>注册对象。</returns>
		public CancellationTokenRegistration Register (Action callback, bool useSynchronizationContext)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			return Source.Register (callback, useSynchronizationContext);
		}

		/// <summary>
		/// 注册带状态对象的取消回调（不捕获同步上下文）。
		/// </summary>
		/// <param name="callback">取消回调（接收状态对象）。</param>
		/// <param name="state">状态对象。</param>
		/// <returns>注册对象。</returns>
		public CancellationTokenRegistration Register (Action<object> callback, object state)
		{
			return Register (callback, state, false);
		}

		/// <summary>
		/// 注册带状态对象的取消回调（可选捕获同步上下文）。
		/// </summary>
		/// <param name="callback">取消回调。</param>
		/// <param name="state">状态对象。</param>
		/// <param name="useSynchronizationContext">是否在同步上下文上执行回调。</param>
		/// <returns>注册对象。</returns>
		public CancellationTokenRegistration Register (Action<object> callback, object state, bool useSynchronizationContext)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			return Register (() => callback (state), useSynchronizationContext);
		}

		/// <summary>
		/// 若已请求取消则抛出 OperationCanceledException。
		/// </summary>
		public void ThrowIfCancellationRequested ()
		{
			if (Source.IsCancellationRequested){
				// throw new OperationCanceledException (this);
				throw new OperationCanceledExceptionDerived(this);
			}
		}

		/// <summary>
		/// 判断两个取消令牌是否相等（按取消源）。
		/// </summary>
		/// <param name="other">另一个取消令牌。</param>
		/// <returns>相等返回 true。</returns>
		public bool Equals (CancellationToken other)
		{
			return this.Source == other.Source;
		}

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
		/// <param name="other">比较对象。</param>
		/// <returns>相等返回 true。</returns>
		public override bool Equals (object other)
		{
			return (other is CancellationToken) ? Equals ((CancellationToken)other) : false;
		}

		/// <summary>
		/// 生成哈希码。
		/// </summary>
		/// <returns>哈希码。</returns>
		public override int GetHashCode ()
		{
			return Source.GetHashCode ();
		}

		/// <summary>相等运算符。</summary>
		public static bool operator == (CancellationToken left, CancellationToken right)
		{
			return left.Equals (right);
		}

		/// <summary>不等运算符。</summary>
		public static bool operator != (CancellationToken left, CancellationToken right)
		{
			return !left.Equals (right);
		}

		/// <summary>
		/// 是否可取消（绑定了非空取消源）。
		/// </summary>
		public bool CanBeCanceled {
			get {
				return source != null;
			}
		}

		/// <summary>
		/// 是否已请求取消。
		/// </summary>
		public bool IsCancellationRequested {
			get {
				return Source.IsCancellationRequested;
			}
		}

		/// <summary>
		/// 关联的等待句柄（取消时被触发）。
		/// </summary>
		public WaitHandle WaitHandle {
			get {
				return Source.WaitHandle;
			}
		}

		/// <summary>
		/// 获取取消源（空令牌使用 NoneSource）。
		/// </summary>
		CancellationTokenSource Source {
			get {
				return source ?? CancellationTokenSource.NoneSource;
			}
		}
	}
}
//#endif

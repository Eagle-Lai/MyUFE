// 
// CancellationTokenRegistration.cs
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
using System.Threading;

/// <summary>
/// 取消回调注册（CancellationTokenRegistration）。
/// <para>用途：从 Mono 移植——表示一次取消回调注册，调用 Dispose 可从取消源移除对应回调。</para>
/// </summary>
namespace System.Threading
{
	/// <summary>
	/// 取消回调注册结构：可 Dispose 取消注册，支持相等比较。
	/// </summary>
	public struct CancellationTokenRegistration: IDisposable, IEquatable<CancellationTokenRegistration>
	{
		/// <summary>注册 ID。</summary>
		int id;
		/// <summary>关联的取消源。</summary>
		CancellationTokenSource source;

		/// <summary>
		/// 内部构造函数。
		/// </summary>
		/// <param name="id">注册 ID。</param>
		/// <param name="source">取消源。</param>
		internal CancellationTokenRegistration (int id, CancellationTokenSource source)
		{
			this.id = id;
			this.source = source;
		}

		#region IDisposable implementation
		/// <summary>
		/// 取消注册：从取消源移除本次注册的回调。
		/// </summary>
		public void Dispose ()
		{
			if (source != null)
				source.RemoveCallback (this);
		}
		#endregion

		#region IEquatable<CancellationTokenRegistration> implementation
		/// <summary>
		/// 判断两个注册是否相等（ID 与取消源均相等）。
		/// </summary>
		/// <param name="other">另一个注册。</param>
		/// <returns>相等返回 true。</returns>
		public bool Equals (CancellationTokenRegistration other)
		{
			return this.id == other.id && this.source == other.source;
		}

		/// <summary>相等运算符。</summary>
		public static bool operator== (CancellationTokenRegistration left, CancellationTokenRegistration right)
		{
			return left.Equals (right);
		}

		/// <summary>不等运算符。</summary>
		public static bool operator!= (CancellationTokenRegistration left, CancellationTokenRegistration right)
		{
			return !left.Equals (right);
		}
		#endregion

		/// <summary>
		/// 生成哈希码。
		/// </summary>
		/// <returns>哈希码。</returns>
		public override int GetHashCode ()
		{
			return id.GetHashCode () ^ (source == null ? 0 : source.GetHashCode ());
		}

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
		/// <param name="obj">比较对象。</param>
		/// <returns>相等返回 true。</returns>
		public override bool Equals (object obj)
		{
			return (obj is CancellationTokenRegistration) ? Equals ((CancellationTokenRegistration)obj) : false;
		}
	}
}
//#endif

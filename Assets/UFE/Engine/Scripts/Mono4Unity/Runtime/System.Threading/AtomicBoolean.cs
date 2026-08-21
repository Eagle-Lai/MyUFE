// AtomicBoolean.cs
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


using System;

#if INSIDE_MONO_PARALLEL
using System.Threading;

namespace Mono.Threading
#else
namespace System.Threading
#endif
{
	/// <summary>
	/// 原子布尔值（AtomicBooleanValue，值类型）。
	/// <para>用途：从 Mono 移植的线程安全布尔值——基于 Interlocked 提供 CompareAndExchange/Exchange 等原子操作，</para>
	/// <para>支持与 bool 的隐式/显式转换。</para>
	/// </summary>
#if INSIDE_MONO_PARALLEL
	public
#endif
	struct AtomicBooleanValue
	{
		/// <summary>底层标志（1=真，0=假）。</summary>
		int flag;
		/// <summary>未设置值。</summary>
		const int UnSet = 0;
		/// <summary>已设置值。</summary>
		const int Set = 1;

		/// <summary>
		/// 原子比较并交换：若当前值等于 expected 则设为 newVal。
		/// </summary>
		/// <param name="expected">期望的旧值。</param>
		/// <param name="newVal">新值。</param>
		/// <returns>交换成功返回 true。</returns>
		public bool CompareAndExchange (bool expected, bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			int expectedTemp = expected ? Set : UnSet;

			return Interlocked.CompareExchange (ref flag, newTemp, expectedTemp) == expectedTemp;
		}

		/// <summary>
		/// 从 bool 值创建原子布尔值。
		/// </summary>
		/// <param name="value">初始值。</param>
		/// <returns>原子布尔值。</returns>
		public static AtomicBooleanValue FromValue (bool value)
		{
			AtomicBooleanValue temp = new AtomicBooleanValue ();
			temp.Value = value;

			return temp;
		}

		/// <summary>
		/// 尝试设置为真（仅在为假时成功）。
		/// </summary>
		/// <returns>设置成功返回 true。</returns>
		public bool TrySet ()
		{
			return !Exchange (true);
		}

		/// <summary>
		/// 宽松尝试设置为真（非阻塞读取后交换）。
		/// </summary>
		/// <returns>设置成功返回 true。</returns>
		public bool TryRelaxedSet ()
		{
			return flag == UnSet && !Exchange (true);
		}

		/// <summary>
		/// 原子交换并返回旧值。
		/// </summary>
		/// <param name="newVal">新值。</param>
		/// <returns>交换前的旧值。</returns>
		public bool Exchange (bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			return Interlocked.Exchange (ref flag, newTemp) == Set;
		}

		/// <summary>
		/// 当前值。
		/// </summary>
		public bool Value {
			get {
				return flag == Set;
			}
			set {
				Exchange (value);
			}
		}

		/// <summary>
		/// 判断两个原子布尔值是否相等。
		/// </summary>
		/// <param name="rhs">另一个原子布尔值。</param>
		/// <returns>相等返回 true。</returns>
		public bool Equals (AtomicBooleanValue rhs)
		{
			return this.flag == rhs.flag;
		}

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
		/// <param name="rhs">比较对象。</param>
		/// <returns>相等返回 true。</returns>
		public override bool Equals (object rhs)
		{
			return rhs is AtomicBooleanValue ? Equals ((AtomicBooleanValue)rhs) : false;
		}

		/// <summary>
		/// 生成哈希码。
		/// </summary>
		/// <returns>哈希码。</returns>
		public override int GetHashCode ()
		{
			return flag.GetHashCode ();
		}

		/// <summary>
		/// 显式转换为 bool。
		/// </summary>
		public static explicit operator bool (AtomicBooleanValue rhs)
		{
			return rhs.Value;
		}

		/// <summary>
		/// 从 bool 隐式转换为原子布尔值。
		/// </summary>
		public static implicit operator AtomicBooleanValue (bool rhs)
		{
			return AtomicBooleanValue.FromValue (rhs);
		}
	}

	/// <summary>
	/// 原子布尔值（AtomicBoolean，类类型）。
	/// <para>用途：从 Mono 移植的线程安全布尔值——基于 Interlocked 提供原子操作，</para>
	/// <para>支持与 bool 的隐式/显式转换。</para>
	/// </summary>
#if INSIDE_MONO_PARALLEL
	public
#endif
	class AtomicBoolean
	{
		/// <summary>底层标志（1=真，0=假）。</summary>
		int flag;
		/// <summary>未设置值。</summary>
		const int UnSet = 0;
		/// <summary>已设置值。</summary>
		const int Set = 1;

		/// <summary>
		/// 原子比较并交换。
		/// </summary>
		/// <param name="expected">期望的旧值。</param>
		/// <param name="newVal">新值。</param>
		/// <returns>交换成功返回 true。</returns>
		public bool CompareAndExchange (bool expected, bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			int expectedTemp = expected ? Set : UnSet;

			return Interlocked.CompareExchange (ref flag, newTemp, expectedTemp) == expectedTemp;
		}

		/// <summary>
		/// 从 bool 值创建原子布尔值。
		/// </summary>
		/// <param name="value">初始值。</param>
		/// <returns>原子布尔值。</returns>
		public static AtomicBoolean FromValue (bool value)
		{
			AtomicBoolean temp = new AtomicBoolean ();
			temp.Value = value;

			return temp;
		}

		/// <summary>
		/// 尝试设置为真（仅在为假时成功）。
		/// </summary>
		/// <returns>设置成功返回 true。</returns>
		public bool TrySet ()
		{
			return !Exchange (true);
		}

		/// <summary>
		/// 宽松尝试设置为真。
		/// </summary>
		/// <returns>设置成功返回 true。</returns>
		public bool TryRelaxedSet ()
		{
			return flag == UnSet && !Exchange (true);
		}

		/// <summary>
		/// 原子交换并返回旧值。
		/// </summary>
		/// <param name="newVal">新值。</param>
		/// <returns>交换前的旧值。</returns>
		public bool Exchange (bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			return Interlocked.Exchange (ref flag, newTemp) == Set;
		}

		/// <summary>
		/// 当前值。
		/// </summary>
		public bool Value {
			get {
				return flag == Set;
			}
			set {
				Exchange (value);
			}
		}

		/// <summary>
		/// 判断两个原子布尔值是否相等。
		/// </summary>
		/// <param name="rhs">另一个原子布尔值。</param>
		/// <returns>相等返回 true。</returns>
		public bool Equals (AtomicBoolean rhs)
		{
			return this.flag == rhs.flag;
		}

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
		/// <param name="rhs">比较对象。</param>
		/// <returns>相等返回 true。</returns>
		public override bool Equals (object rhs)
		{
			return rhs is AtomicBoolean ? Equals ((AtomicBoolean)rhs) : false;
		}

		/// <summary>
		/// 生成哈希码。
		/// </summary>
		/// <returns>哈希码。</returns>
		public override int GetHashCode ()
		{
			return flag.GetHashCode ();
		}

		/// <summary>
		/// 显式转换为 bool。
		/// </summary>
		public static explicit operator bool (AtomicBoolean rhs)
		{
			return rhs.Value;
		}

		/// <summary>
		/// 从 bool 隐式转换为原子布尔值。
		/// </summary>
		public static implicit operator AtomicBoolean (bool rhs)
		{
			return AtomicBoolean.FromValue (rhs);
		}
	}
}

//
// Tuple.cs
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009 Novell
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

//#if MOONLIGHT || NET_4_0 || MOBILE

using System;

/// <summary>
/// 元组工厂（Tuple）。
/// <para>用途：提供创建 1~8 元素元组（Tuple&lt;T1..T8&gt;）的静态工厂方法，从 Mono 移植。</para>
/// <para>当第 8 个元素存在时，自动嵌套 Tuple&lt;T8&gt; 作为第 8 元素以支持 8 元以上元组。</para>
/// </summary>
namespace System
{
	/// <summary>
	/// 元组工厂类：提供各种长度元组的创建方法。
	/// </summary>
	public static class Tuple
	{
		/// <summary>
		/// 创建 8 元素元组（第 8 元素嵌套为单元素元组）。
		/// </summary>
		/// <returns>8 元素元组。</returns>
		/// <param name="item1">元素1。</param>
		/// <param name="item2">元素2。</param>
		/// <param name="item3">元素3。</param>
		/// <param name="item4">元素4。</param>
		/// <param name="item5">元素5。</param>
		/// <param name="item6">元素6。</param>
		/// <param name="item7">元素7。</param>
		/// <param name="item8">元素8。</param>
		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5,
			 T6 item6,
			 T7 item7,
			 T8 item8) {
			return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> (item1, item2, item3, item4, item5, item6, item7, new Tuple<T8> (item8));
		}

		/// <summary>
		/// 创建 7 元素元组。
		/// </summary>
		/// <returns>7 元素元组。</returns>
		public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5,
			 T6 item6,
			 T7 item7) {
			return new Tuple<T1, T2, T3, T4, T5, T6, T7> (item1, item2, item3, item4, item5, item6, item7);
		}

		/// <summary>
		/// 创建 6 元素元组。
		/// </summary>
		/// <returns>6 元素元组。</returns>
		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5,
			 T6 item6) {
			return new Tuple<T1, T2, T3, T4, T5, T6> (item1, item2, item3, item4, item5, item6);
		}

		/// <summary>
		/// 创建 5 元素元组。
		/// </summary>
		/// <returns>5 元素元组。</returns>
		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5) {
			return new Tuple<T1, T2, T3, T4, T5> (item1, item2, item3, item4, item5);
		}

		/// <summary>
		/// 创建 4 元素元组。
		/// </summary>
		/// <returns>4 元素元组。</returns>
		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4) {
			return new Tuple<T1, T2, T3, T4> (item1, item2, item3, item4);
		}

		/// <summary>
		/// 创建 3 元素元组。
		/// </summary>
		/// <returns>3 元素元组。</returns>
		public static Tuple<T1, T2, T3> Create<T1, T2, T3>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3) {
			return new Tuple<T1, T2, T3> (item1, item2, item3);
		}

		/// <summary>
		/// 创建 2 元素元组。
		/// </summary>
		/// <returns>2 元素元组。</returns>
		public static Tuple<T1, T2> Create<T1, T2>
			(
			 T1 item1,
			 T2 item2) {
			return new Tuple<T1, T2> (item1, item2);
		}

		/// <summary>
		/// 创建 1 元素元组。
		/// </summary>
		/// <returns>1 元素元组。</returns>
		public static Tuple<T1> Create<T1>
			(
			 T1 item1) {
			return new Tuple<T1> (item1);
		}
	}		
}

//#endif

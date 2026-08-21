//
// Complex.cs: Complex number support
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//   Marek Safar (marek.safar@gmail.com)
//   Jb Evain (jbevain@novell.com)
//
// Copyright 2009, 2010 Novell, Inc.
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
using System;

/// <summary>
/// 复数（Complex）。
/// <para>用途：从 Mono 移植的复数结构——以 double 实数/虚数分量表示复数，</para>
/// <para>提供加减乘除、模长/相位、共轭、三角/指数/对数函数及运算符。</para>
/// </summary>
namespace System.Numerics
{
	/// <summary>
	/// 复数结构体。
	/// </summary>
	public struct Complex : IEquatable<Complex>, IFormattable
	{
		/// <summary>实数分量。</summary>
		double real, imaginary;
		/// <summary>虚数单位 i（0,1）。</summary>
		public static readonly Complex ImaginaryOne = new Complex (0, 1);
		/// <summary>复数 1。</summary>
		public static readonly Complex One = new Complex (1, 0);
		/// <summary>复数 0。</summary>
		public static readonly Complex Zero = new Complex (0, 0);

		/// <summary>虚数分量。</summary>
		public double Imaginary {
			get { return imaginary; }
		}

		/// <summary>实数分量。</summary>
		public double Real {
			get { return real; }
		}

		/// <summary>模长（幅值）。</summary>
		public double Magnitude {
			get { return Math.Sqrt (imaginary * imaginary + real * real); }
		}

		/// <summary>相位角（辐角）。</summary>
		public double Phase {
			get { return Math.Atan2 (imaginary, real); }
		}

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="real">实数分量。</param>
		/// <param name="imaginary">虚数分量。</param>
		public Complex (double real, double imaginary)
		{
			this.imaginary = imaginary;
			this.real = real;
		}

		/// <summary>
		/// 共轭复数。
		/// </summary>
		/// <param name="c">源复数。</param>
		/// <returns>共轭复数。</returns>
		public static Complex Conjugate (Complex c)
		{
			return new Complex (c.real, -c.imaginary);
		}

		/// <summary>
		/// 加法运算。
		/// </summary>
		/// <param name="left">左操作数。</param>
		/// <param name="right">右操作数。</param>
		/// <returns>和。</returns>
		public static Complex operator + (Complex left, Complex right)
		{
			return new Complex (left.real + right.real, left.imaginary + right.imaginary);
		}

		/// <summary>
		/// 减法运算。
		/// </summary>
		/// <param name="left">左操作数。</param>
		/// <param name="right">右操作数。</param>
		/// <returns>差。</returns>
		public static Complex operator - (Complex left, Complex right)
		{
			return new Complex (left.real - right.real, left.imaginary - right.imaginary);
		}

		/// <summary>
		/// 乘法运算。
		/// </summary>
		/// <param name="left">左操作数。</param>
		/// <param name="right">右操作数。</param>
		/// <returns>积。</returns>
		public static Complex operator * (Complex left, Complex right)
		{
			return new Complex (
				left.real * right.real - left.imaginary * right.imaginary,
				left.imaginary * right.real + left.real * right.imaginary);
		}

		/// <summary>
		/// 除法运算。
		/// </summary>
		/// <param name="left">被除数。</param>
		/// <param name="right">除数。</param>
		/// <returns>商。</returns>
		public static Complex operator / (Complex left, Complex right)
		{
			double denom = right.real * right.real + right.imaginary * right.imaginary;
			return new Complex (
				(left.real * right.real + left.imaginary * right.imaginary) / denom,
				(left.imaginary * right.real - left.real * right.imaginary) / denom);
		}

		/// <summary>
		/// 一元取反运算。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>取反后的复数。</returns>
		public static Complex operator - (Complex value)
		{
			return new Complex (-value.real, -value.imaginary);
		}

		/// <summary>
		/// 双精度数与复数加法。
		/// </summary>
		public static Complex operator + (Complex left, double right)
		{
			return new Complex (left.real + right, left.imaginary);
		}

		/// <summary>
		/// 双精度数与复数加法（交换律）。
		/// </summary>
		public static Complex operator + (double left, Complex right)
		{
			return new Complex (left + right.real, right.imaginary);
		}

		/// <summary>
		/// 双精度数与复数减法。
		/// </summary>
		public static Complex operator - (Complex left, double right)
		{
			return new Complex (left.real - right, left.imaginary);
		}

		/// <summary>
		/// 双精度数减复数。
		/// </summary>
		public static Complex operator - (double left, Complex right)
		{
			return new Complex (left - right.real, right.imaginary);
		}

		/// <summary>
		/// 双精度数与复数乘法。
		/// </summary>
		public static Complex operator * (Complex left, double right)
		{
			return new Complex (left.real * right, left.imaginary * right);
		}

		/// <summary>
		/// 双精度数与复数乘法（交换律）。
		/// </summary>
		public static Complex operator * (double left, Complex right)
		{
			return new Complex (left * right.real, left * right.imaginary);
		}

		/// <summary>
		/// 双精度数与复数除法。
		/// </summary>
		public static Complex operator / (Complex left, double right)
		{
			return new Complex (left.real / right, left.imaginary / right);
		}

		/// <summary>
		/// 双精度数除以复数。
		/// </summary>
		public static Complex operator / (double left, Complex right)
		{
			double denom = right.real * right.real + right.imaginary * right.imaginary;
			return new Complex (left * right.real / denom, -left * right.imaginary / denom);
		}

		/// <summary>
		/// 复数转为字符串（默认格式）。
		/// </summary>
		/// <returns>字符串。</returns>
		public override string ToString ()
		{
			return String.Format ("{0} + {1}i", real, imaginary);
		}

		/// <summary>
		/// 按格式字符串格式化复数。
		/// </summary>
		/// <param name="format">格式字符串。</param>
		/// <returns>格式化后的字符串。</returns>
		public string ToString (string format)
		{
			return String.Format ("{0} + {1}i", real.ToString (format), imaginary.ToString (format));
		}

		/// <summary>
		/// 按格式与区域信息格式化复数（IFormattable 实现）。
		/// </summary>
		/// <param name="format">格式字符串。</param>
		/// <param name="provider">区域格式提供者。</param>
		/// <returns>格式化后的字符串。</returns>
		public string ToString (string format, IFormatProvider provider)
		{
			return String.Format ("{0} + {1}i", real.ToString (format, provider), imaginary.ToString (format, provider));
		}

		/// <summary>
		/// 生成哈希码。
		/// </summary>
		/// <returns>哈希码。</returns>
		public override int GetHashCode ()
		{
			return real.GetHashCode () ^ imaginary.GetHashCode ();
		}

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
		/// <param name="other">比较对象。</param>
		/// <returns>相等返回 true。</returns>
		public override bool Equals (object other)
		{
			return (other is Complex) ? Equals ((Complex)other) : false;
		}

		/// <summary>
		/// 判断两个复数是否相等。
		/// </summary>
		/// <param name="other">另一个复数。</param>
		/// <returns>相等返回 true。</returns>
		public bool Equals (Complex other)
		{
			return real.Equals (other.real) && imaginary.Equals (other.imaginary);
		}

		/// <summary>
		/// 从双精度数隐式转换为复数。
		/// </summary>
		public static implicit operator Complex (double value)
		{
			return new Complex (value, 0);
		}

		/// <summary>
		/// 从整数隐式转换为复数。
		/// </summary>
		public static implicit operator Complex (int value)
		{
			return new Complex (value, 0);
		}

		/// <summary>
		/// 从单精度数隐式转换为复数。
		/// </summary>
		public static implicit operator Complex (float value)
		{
			return new Complex (value, 0);
		}

		/// <summary>
		/// 取对数（自然对数，或按底数）。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <param name="baseValue">可选底数（默认自然对数）。</param>
		/// <returns>对数复数。</returns>
		public static Complex Log (Complex value, double baseValue)
		{
			return Complex.Log (value) / Math.Log (baseValue);
		}

		/// <summary>
		/// 自然对数。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>对数复数。</returns>
		public static Complex Log (Complex value)
		{
			return new Complex (Math.Log (value.Magnitude), value.Phase);
		}

		/// <summary>
		/// 以 10 为底的对数。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>对数复数。</returns>
		public static Complex Log10 (Complex value)
		{
			return Complex.Log (value, 10);
		}

		/// <summary>
		/// 复数幂。
		/// </summary>
		/// <param name="left">底数。</param>
		/// <param name="right">指数。</param>
		/// <returns>幂复数。</returns>
		public static Complex Pow (Complex left, Complex right)
		{
			if (left == Complex.Zero && right.Real > 0)
				return Complex.Zero;
			if (left == Complex.Zero && right.Real == 0)
				return Complex.One;

			// a^b = exp (b * log (a))
			return Complex.Exp (right * Complex.Log (left));
		}

		/// <summary>
		/// 复数幂（双精度指数）。
		/// </summary>
		/// <param name="left">底数。</param>
		/// <param name="right">指数。</param>
		/// <returns>幂复数。</returns>
		public static Complex Pow (Complex left, double right)
		{
			return Complex.Pow (left, new Complex (right, 0));
		}

		/// <summary>
		/// 双精度数的复数指数幂。
		/// </summary>
		/// <param name="left">底数。</param>
		/// <param name="right">复数指数。</param>
		/// <returns>幂复数。</returns>
		public static Complex Pow (double left, Complex right)
		{
			return Complex.Pow (new Complex (left, 0), right);
		}

		/// <summary>
		/// 复数指数（e^x）。
		/// </summary>
		/// <param name="value">指数。</param>
		/// <returns>指数复数。</returns>
		public static Complex Exp (Complex value)
		{
			// e^(a + bi) = e^a * (cos b + i sin b)
			return new Complex (
				Math.Exp (value.Real) * Math.Cos (value.Imaginary),
				Math.Exp (value.Real) * Math.Sin (value.Imaginary));
		}

		/// <summary>
		/// 复数正弦。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>正弦复数。</returns>
		public static Complex Sin (Complex value)
		{
			return new Complex (
				Math.Sin (value.Real) * Math.Cosh (value.Imaginary),
				Math.Cos (value.Real) * Math.Sinh (value.Imaginary));
		}

		/// <summary>
		/// 复数余弦。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>余弦复数。</returns>
		public static Complex Cos (Complex value)
		{
			return new Complex (
				Math.Cos (value.Real) * Math.Cosh (value.Imaginary),
				-Math.Sin (value.Real) * Math.Sinh (value.Imaginary));
		}

		/// <summary>
		/// 复数正切。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>正切复数。</returns>
		public static Complex Tan (Complex value)
		{
			return Complex.Sin (value) / Complex.Cos (value);
		}

		/// <summary>
		/// 复数反正弦。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>反正弦复数。</returns>
		public static Complex Asin (Complex value)
		{
			return -Complex.ImaginaryOne * Complex.Log (
				Complex.ImaginaryOne * value + Complex.Sqrt (Complex.One - value * value));
		}

		/// <summary>
		/// 复数反余弦。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>反余弦复数。</returns>
		public static Complex Acos (Complex value)
		{
			return -Complex.ImaginaryOne * Complex.Log (
				value + Complex.ImaginaryOne * Complex.Sqrt (Complex.One - value * value));
		}

		/// <summary>
		/// 复数反正切。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>反正切复数。</returns>
		public static Complex Atan (Complex value)
		{
			return (new Complex (0, 0.5)) * (
				Complex.Log (Complex.One - Complex.ImaginaryOne * value)
				- Complex.Log (Complex.One + Complex.ImaginaryOne * value));
		}

		/// <summary>
		/// 复数平方根。
		/// </summary>
		/// <param name="value">源复数。</param>
		/// <returns>平方根复数。</returns>
		public static Complex Sqrt (Complex value)
		{
			// Use the polar form
			return new Complex (
				Math.Sqrt (value.Magnitude) * Math.Cos (value.Phase / 2),
				Math.Sqrt (value.Magnitude) * Math.Sin (value.Phase / 2));
		}

		/// <summary>相等运算符。</summary>
		public static bool operator == (Complex left, Complex right)
		{
			return left.Equals (right);
		}

		/// <summary>不等运算符。</summary>
		public static bool operator != (Complex left, Complex right)
		{
			return !left.Equals (right);
		}
	}
}

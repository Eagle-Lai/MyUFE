#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors
 * Alan McGovern

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion License

using System;

namespace FPLibrary {

    /// <summary>
    /// 定点二维向量（FPVector2）。
    /// <para>用途：以两个 Fix64 分量（x/y）表示二维向量，提供加减乘除、点积、距离、反射、插值、</para>
    /// <para>归一化等运算，保证网络对战确定性。</para>
    /// </summary>

    /// <summary>
    /// 定点二维向量结构体。
    /// </summary>
    [Serializable]
    public struct FPVector2 : IEquatable<FPVector2>
    {
#region Private Fields
		/// <summary>零向量（0,0）静态缓存。</summary>
        private static FPVector2 zeroVector = new FPVector2(0, 0);
		/// <summary>单位向量（1,1）静态缓存。</summary>
        private static FPVector2 oneVector = new FPVector2(1, 1);

		/// <summary>右向量（1,0）静态缓存。</summary>
        private static FPVector2 rightVector = new FPVector2(1, 0);
		/// <summary>左向量（-1,0）静态缓存。</summary>
        private static FPVector2 leftVector = new FPVector2(-1, 0);

		/// <summary>上向量（0,1）静态缓存。</summary>
        private static FPVector2 upVector = new FPVector2(0, 1);
		/// <summary>下向量（0,-1）静态缓存。</summary>
        private static FPVector2 downVector = new FPVector2(0, -1);

        #endregion Private Fields

        #region Public Fields
		/// <summary>X 分量。</summary>
        public Fix64 x;
		/// <summary>Y 分量。</summary>
        public Fix64 y;

        #endregion Public Fields

#region Properties
		/// <summary>零向量（0,0）。</summary>
        public static FPVector2 zero
        {
            get { return zeroVector; }
        }

		/// <summary>单位向量（1,1）。</summary>
        public static FPVector2 one
        {
            get { return oneVector; }
        }

		/// <summary>右向量（1,0）。</summary>
        public static FPVector2 right
        {
            get { return rightVector; }
        }

		/// <summary>左向量（-1,0）。</summary>
        public static FPVector2 left {
            get { return leftVector; }
        }

		/// <summary>上向量（0,1）。</summary>
        public static FPVector2 up
        {
            get { return upVector; }
        }

		/// <summary>下向量（0,-1）。</summary>
        public static FPVector2 down {
            get { return downVector; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor foe standard 2D vector.
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/>
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/>
        /// </param>
        /// <summary>构造函数（指定分量）。</summary>
        /// <param name="x">X 分量。</param>
        /// <param name="y">Y 分量。</param>
        public FPVector2(Fix64 x, Fix64 y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Constructor for "square" vector.
        /// </summary>
        /// <param name="value">
        /// A <see cref="System.Single"/>
        /// </param>
        /// <summary>构造函数（两分量相同）。</summary>
        /// <param name="value">分量值。</param>
        public FPVector2(Fix64 value)
        {
            x = value;
            y = value;
        }

		/// <summary>
		/// 设置向量分量。
		/// </summary>
		/// <param name="x">新 X 分量。</param>
		/// <param name="y">新 Y 分量。</param>
        public void Set(Fix64 x, Fix64 y) {
            this.x = x;
            this.y = y;
        }

        #endregion Constructors

        #region Public Methods

		/// <summary>
		/// 沿法线反射向量（引用版本）。
		/// </summary>
		/// <param name="vector">入射向量。</param>
		/// <param name="normal">反射法线。</param>
		/// <param name="result">反射结果。</param>
        public static void Reflect(ref FPVector2 vector, ref FPVector2 normal, out FPVector2 result)
        {
            Fix64 dot = Dot(vector, normal);
            result.x = vector.x - ((2f*dot)*normal.x);
            result.y = vector.y - ((2f*dot)*normal.y);
        }

		/// <summary>
		/// 沿法线反射向量（返回新向量）。
		/// </summary>
		/// <param name="vector">入射向量。</param>
		/// <param name="normal">反射法线。</param>
		/// <returns>反射结果向量。</returns>
        public static FPVector2 Reflect(FPVector2 vector, FPVector2 normal)
        {
            FPVector2 result;
            Reflect(ref vector, ref normal, out result);
            return result;
        }

		/// <summary>
		/// 向量相加。
		/// </summary>
		/// <param name="value1">向量1。</param>
		/// <param name="value2">向量2。</param>
		/// <returns>和向量。</returns>
        public static FPVector2 Add(FPVector2 value1, FPVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

		/// <summary>
		/// 向量相加（引用版本）。
		/// </summary>
		/// <param name="value1">向量1。</param>
		/// <param name="value2">向量2。</param>
		/// <param name="result">和向量。</param>
        public static void Add(ref FPVector2 value1, ref FPVector2 value2, out FPVector2 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
        }

		/// <summary>
		/// 重心坐标插值。
		/// </summary>
		/// <param name="value1">顶点值1。</param>
		/// <param name="value2">顶点值2。</param>
		/// <param name="value3">顶点值3。</param>
		/// <param name="amount1">权重1。</param>
		/// <param name="amount2">权重2。</param>
		/// <returns>插值结果。</returns>
        public static FPVector2 Barycentric(FPVector2 value1, FPVector2 value2, FPVector2 value3, Fix64 amount1, Fix64 amount2)
        {
            return new FPVector2(
                FPMath.Barycentric(value1.x, value2.x, value3.x, amount1, amount2),
                FPMath.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

		/// <summary>
		/// 重心坐标插值（引用版本）。
		/// </summary>
        public static void Barycentric(ref FPVector2 value1, ref FPVector2 value2, ref FPVector2 value3, Fix64 amount1,
                                       Fix64 amount2, out FPVector2 result)
        {
            result = new FPVector2(
                FPMath.Barycentric(value1.x, value2.x, value3.x, amount1, amount2),
                FPMath.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

		/// <summary>
		/// Catmull-Rom 样条插值。
		/// </summary>
        public static FPVector2 CatmullRom(FPVector2 value1, FPVector2 value2, FPVector2 value3, FPVector2 value4, Fix64 amount)
        {
            return new FPVector2(
                FPMath.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                FPMath.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

		/// <summary>
		/// Catmull-Rom 样条插值（引用版本）。
		/// </summary>
        public static void CatmullRom(ref FPVector2 value1, ref FPVector2 value2, ref FPVector2 value3, ref FPVector2 value4,
                                      Fix64 amount, out FPVector2 result)
        {
            result = new FPVector2(
                FPMath.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                FPMath.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

		/// <summary>
		/// 分量限制到 [min, max]。
		/// </summary>
        public static FPVector2 Clamp(FPVector2 value1, FPVector2 min, FPVector2 max)
        {
            return new FPVector2(
                FPMath.Clamp(value1.x, min.x, max.x),
                FPMath.Clamp(value1.y, min.y, max.y));
        }

		/// <summary>
		/// 分量限制（引用版本）。
		/// </summary>
        public static void Clamp(ref FPVector2 value1, ref FPVector2 min, ref FPVector2 max, out FPVector2 result)
        {
            result = new FPVector2(
                FPMath.Clamp(value1.x, min.x, max.x),
                FPMath.Clamp(value1.y, min.y, max.y));
        }

        /// <summary>
        /// Returns FP precison distanve between two vectors
        /// </summary>
        /// <param name="value1">
        /// A <see cref="FPVector2"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="FPVector2"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Single"/>
        /// </returns>
        /// <summary>计算两点欧氏距离。</summary>
        /// <param name="value1">点1。</param>
        /// <param name="value2">点2。</param>
        /// <returns>距离值。</returns>
        public static Fix64 Distance(FPVector2 value1, FPVector2 value2)
        {
            Fix64 result;
            DistanceSquared(ref value1, ref value2, out result);
            return (Fix64) Fix64.Sqrt(result);
        }

		/// <summary>
		/// 计算两点欧氏距离（引用版本）。
		/// </summary>
        public static void Distance(ref FPVector2 value1, ref FPVector2 value2, out Fix64 result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (Fix64) Fix64.Sqrt(result);
        }

		/// <summary>
		/// 计算两点距离平方。
		/// </summary>
        public static Fix64 DistanceSquared(FPVector2 value1, FPVector2 value2)
        {
            Fix64 result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

		/// <summary>
		/// 计算两点距离平方（引用版本）。
		/// </summary>
        public static void DistanceSquared(ref FPVector2 value1, ref FPVector2 value2, out Fix64 result)
        {
            result = (value1.x - value2.x)*(value1.x - value2.x) + (value1.y - value2.y)*(value1.y - value2.y);
        }

        /// <summary>
        /// Devide first vector with the secund vector
        /// </summary>
        /// <param name="value1">
        /// A <see cref="FPVector2"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="FPVector2"/>
        /// </param>
        /// <returns>
        /// A <see cref="FPVector2"/>
        /// </returns>
        /// <summary>向量分量相除。</summary>
        public static FPVector2 Divide(FPVector2 value1, FPVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

		/// <summary>
		/// 向量分量相除（引用版本）。
		/// </summary>
        public static void Divide(ref FPVector2 value1, ref FPVector2 value2, out FPVector2 result)
        {
            result.x = value1.x/value2.x;
            result.y = value1.y/value2.y;
        }

		/// <summary>
		/// 向量除以标量。
		/// </summary>
        public static FPVector2 Divide(FPVector2 value1, Fix64 divider)
        {
            Fix64 factor = 1/divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

		/// <summary>
		/// 向量除以标量（引用版本）。
		/// </summary>
        public static void Divide(ref FPVector2 value1, Fix64 divider, out FPVector2 result)
        {
            Fix64 factor = 1/divider;
            result.x = value1.x*factor;
            result.y = value1.y*factor;
        }

		/// <summary>
		/// 计算点积。
		/// </summary>
        public static Fix64 Dot(FPVector2 value1, FPVector2 value2)
        {
            return value1.x*value2.x + value1.y*value2.y;
        }

		/// <summary>
		/// 计算点积（引用版本）。
		/// </summary>
        public static void Dot(ref FPVector2 value1, ref FPVector2 value2, out Fix64 result)
        {
            result = value1.x*value2.x + value1.y*value2.y;
        }

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
        public override bool Equals(object obj)
        {
            return (obj is FPVector2) ? this == ((FPVector2) obj) : false;
        }

		/// <summary>
		/// 判断两个向量是否相等。
		/// </summary>
        public bool Equals(FPVector2 other)
        {
            return this == other;
        }

		/// <summary>
		/// 生成哈希码。
		/// </summary>
        public override int GetHashCode()
        {
            return (int) (x + y);
        }

		/// <summary>
		/// Hermite 样条插值。
		/// </summary>
        public static FPVector2 Hermite(FPVector2 value1, FPVector2 tangent1, FPVector2 value2, FPVector2 tangent2, Fix64 amount)
        {
            FPVector2 result = new FPVector2();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

		/// <summary>
		/// Hermite 样条插值（引用版本）。
		/// </summary>
        public static void Hermite(ref FPVector2 value1, ref FPVector2 tangent1, ref FPVector2 value2, ref FPVector2 tangent2,
                                   Fix64 amount, out FPVector2 result)
        {
            result.x = FPMath.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
            result.y = FPMath.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
        }

		/// <summary>
		/// 向量长度（模长）。
		/// </summary>
        public Fix64 magnitude {
            get {
                Fix64 result;
                DistanceSquared(ref this, ref zeroVector, out result);
                return Fix64.Sqrt(result);
            }
        }

		/// <summary>
		/// 将向量归一化后按最大长度缩放。
		/// </summary>
        public static FPVector2 ClampMagnitude(FPVector2 vector, Fix64 maxLength) {
            return Normalize(vector) * maxLength;
        }

		/// <summary>
		/// 向量长度平方。
		/// </summary>
        public Fix64 LengthSquared()
        {
            Fix64 result;
            DistanceSquared(ref this, ref zeroVector, out result);
            return result;
        }

		/// <summary>
		/// 线性插值（参数夹取到 0~1）。
		/// </summary>
        public static FPVector2 Lerp(FPVector2 value1, FPVector2 value2, Fix64 amount) {
            amount = FPMath.Clamp(amount, 0, 1);

            return new FPVector2(
                FPMath.Lerp(value1.x, value2.x, amount),
                FPMath.Lerp(value1.y, value2.y, amount));
        }

		/// <summary>
		/// 线性插值（不夹取参数）。
		/// </summary>
        public static FPVector2 LerpUnclamped(FPVector2 value1, FPVector2 value2, Fix64 amount)
        {
            return new FPVector2(
                FPMath.Lerp(value1.x, value2.x, amount),
                FPMath.Lerp(value1.y, value2.y, amount));
        }

		/// <summary>
		/// 线性插值（不夹取参数，引用版本）。
		/// </summary>
        public static void LerpUnclamped(ref FPVector2 value1, ref FPVector2 value2, Fix64 amount, out FPVector2 result)
        {
            result = new FPVector2(
                FPMath.Lerp(value1.x, value2.x, amount),
                FPMath.Lerp(value1.y, value2.y, amount));
        }

		/// <summary>
		/// 取两向量各分量较大值。
		/// </summary>
        public static FPVector2 Max(FPVector2 value1, FPVector2 value2)
        {
            return new FPVector2(
                FPMath.Max(value1.x, value2.x),
                FPMath.Max(value1.y, value2.y));
        }

		/// <summary>
		/// 取两向量各分量较大值（引用版本）。
		/// </summary>
        public static void Max(ref FPVector2 value1, ref FPVector2 value2, out FPVector2 result)
        {
            result.x = FPMath.Max(value1.x, value2.x);
            result.y = FPMath.Max(value1.y, value2.y);
        }

		/// <summary>
		/// 取两向量各分量较小值。
		/// </summary>
        public static FPVector2 Min(FPVector2 value1, FPVector2 value2)
        {
            return new FPVector2(
                FPMath.Min(value1.x, value2.x),
                FPMath.Min(value1.y, value2.y));
        }

		/// <summary>
		/// 取两向量各分量较小值（引用版本）。
		/// </summary>
        public static void Min(ref FPVector2 value1, ref FPVector2 value2, out FPVector2 result)
        {
            result.x = FPMath.Min(value1.x, value2.x);
            result.y = FPMath.Min(value1.y, value2.y);
        }

		/// <summary>
		/// 分量缩放。
		/// </summary>
        public void Scale(FPVector2 other) {
            this.x = x * other.x;
            this.y = y * other.y;
        }

		/// <summary>
		/// 分量缩放（返回新向量）。
		/// </summary>
        public static FPVector2 Scale(FPVector2 value1, FPVector2 value2) {
            FPVector2 result;
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;

            return result;
        }

		/// <summary>
		/// 向量分量相乘。
		/// </summary>
        public static FPVector2 Multiply(FPVector2 value1, FPVector2 value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            return value1;
        }

		/// <summary>
		/// 向量乘以标量。
		/// </summary>
        public static FPVector2 Multiply(FPVector2 value1, Fix64 scaleFactor)
        {
            value1.x *= scaleFactor;
            value1.y *= scaleFactor;
            return value1;
        }

		/// <summary>
		/// 向量乘以标量（引用版本）。
		/// </summary>
        public static void Multiply(ref FPVector2 value1, Fix64 scaleFactor, out FPVector2 result)
        {
            result.x = value1.x*scaleFactor;
            result.y = value1.y*scaleFactor;
        }

		/// <summary>
		/// 向量分量相乘（引用版本）。
		/// </summary>
        public static void Multiply(ref FPVector2 value1, ref FPVector2 value2, out FPVector2 result)
        {
            result.x = value1.x*value2.x;
            result.y = value1.y*value2.y;
        }

		/// <summary>
		/// 向量取反。
		/// </summary>
        public static FPVector2 Negate(FPVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

		/// <summary>
		/// 向量取反（引用版本）。
		/// </summary>
        public static void Negate(ref FPVector2 value, out FPVector2 result)
        {
            result.x = -value.x;
            result.y = -value.y;
        }

		/// <summary>
		/// 归一化当前向量。
		/// </summary>
        public void Normalize()
        {
            Normalize(ref this, out this);
        }

		/// <summary>
		/// 归一化向量（返回新向量）。
		/// </summary>
        public static FPVector2 Normalize(FPVector2 value)
        {
            Normalize(ref value, out value);
            return value;
        }

		/// <summary>
		/// 归一化向量属性。
		/// </summary>
        public FPVector2 normalized {
            get {
                FPVector2 result;
                FPVector2.Normalize(ref this, out result);

                return result;
            }
        }

		/// <summary>
		/// 归一化向量（引用版本）。
		/// </summary>
        public static void Normalize(ref FPVector2 value, out FPVector2 result)
        {
            Fix64 factor;
            DistanceSquared(ref value, ref zeroVector, out factor);
            factor = 1f/(Fix64) Fix64.Sqrt(factor);
            result.x = value.x*factor;
            result.y = value.y*factor;
        }

		/// <summary>
		/// 平滑插值。
		/// </summary>
        public static FPVector2 SmoothStep(FPVector2 value1, FPVector2 value2, Fix64 amount)
        {
            return new FPVector2(
                FPMath.SmoothStep(value1.x, value2.x, amount),
                FPMath.SmoothStep(value1.y, value2.y, amount));
        }

		/// <summary>
		/// 平滑插值（引用版本）。
		/// </summary>
        public static void SmoothStep(ref FPVector2 value1, ref FPVector2 value2, Fix64 amount, out FPVector2 result)
        {
            result = new FPVector2(
                FPMath.SmoothStep(value1.x, value2.x, amount),
                FPMath.SmoothStep(value1.y, value2.y, amount));
        }

		/// <summary>
		/// 向量相减。
		/// </summary>
        public static FPVector2 Subtract(FPVector2 value1, FPVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

		/// <summary>
		/// 向量相减（引用版本）。
		/// </summary>
        public static void Subtract(ref FPVector2 value1, ref FPVector2 value2, out FPVector2 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
        }

		/// <summary>
		/// 计算两向量夹角（度）。
		/// </summary>
        public static Fix64 Angle(FPVector2 a, FPVector2 b) {
            return Fix64.Acos(a.normalized * b.normalized) * Fix64.Rad2Deg;
        }

		/// <summary>
		/// 转换为三维定点向量（Z 为 0）。
		/// </summary>
        public FPVector ToFPVector() {
            return new FPVector(this.x, this.y, 0);
        }

		/// <summary>
		/// 生成可读的调试字符串。
		/// </summary>
        public override string ToString() {
            return string.Format("({0:f1}, {1:f1})", x.AsFloat(), y.AsFloat());
        }

        #endregion Public Methods

#region Operators

		/// <summary>一元负号运算符。</summary>
        public static FPVector2 operator -(FPVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

		/// <summary>相等运算符。</summary>
        public static bool operator ==(FPVector2 value1, FPVector2 value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }

		/// <summary>不相等运算符。</summary>
        public static bool operator !=(FPVector2 value1, FPVector2 value2)
        {
            return value1.x != value2.x || value1.y != value2.y;
        }

		/// <summary>加法运算符。</summary>
        public static FPVector2 operator +(FPVector2 value1, FPVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

		/// <summary>减法运算符。</summary>
        public static FPVector2 operator -(FPVector2 value1, FPVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

		/// <summary>点积运算符。</summary>
        public static Fix64 operator *(FPVector2 value1, FPVector2 value2)
        {
            return FPVector2.Dot(value1, value2);
        }

		/// <summary>向量乘以标量运算符。</summary>
        public static FPVector2 operator *(FPVector2 value, Fix64 scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

		/// <summary>标量乘以向量运算符。</summary>
        public static FPVector2 operator *(Fix64 scaleFactor, FPVector2 value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

		/// <summary>向量分量相除运算符。</summary>
        public static FPVector2 operator /(FPVector2 value1, FPVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

		/// <summary>向量除以标量运算符。</summary>
        public static FPVector2 operator /(FPVector2 value1, Fix64 divider)
        {
            Fix64 factor = 1/divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        #endregion Operators
    }
}

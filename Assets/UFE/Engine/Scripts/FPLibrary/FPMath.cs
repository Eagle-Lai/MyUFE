/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

namespace FPLibrary {

    /// <summary>
    /// 定点数学工具类（FPMath）。
    /// <para>用途：提供基于 Fix64 的常用数学运算封装（平方根、取整、三角函数、对数、幂、插值、距离等），</para>
    /// <para>全部委托给 Fix64 实现，供引擎各处使用以保证网络确定性。</para>
    /// </summary>

    /// <summary>
    /// Contains common math operations.
    /// </summary>
    /// <summary>定点数常用数学运算集合。</summary>
    public sealed class FPMath {

        /// <summary>
        /// PI constant.
        /// </summary>
        /// <summary>圆周率 π。</summary>
        public static Fix64 Pi = Fix64.Pi;

        /**
        *  @brief PI over 2 constant.
        **/
        /// <summary>π/2。</summary>
        public static Fix64 PiOver2 = Fix64.PiOver2;

        /// <summary>
        /// A small value often used to decide if numeric 
        /// results are zero.
        /// </summary>
        /// <summary>极小值（用于判断数值是否为零）。</summary>
		public static Fix64 Epsilon = Fix64.Epsilon;

        /**
        *  @brief Degree to radians constant.
        **/
        /// <summary>角度转弧度系数。</summary>
        public static Fix64 Deg2Rad = Fix64.Deg2Rad;

        /**
        *  @brief Radians to degree constant.
        **/
        /// <summary>弧度转角度系数。</summary>
        public static Fix64 Rad2Deg = Fix64.Rad2Deg;

        /// <summary>
        /// Gets the square root.
        /// </summary>
        /// <param name="number">The number to get the square root from.</param>
        /// <returns></returns>
        /// <summary>计算平方根。</summary>
        /// <param name="number">要开方的数。</param>
        /// <returns>平方根结果。</returns>
        #region public static FP Sqrt(FP number)
        public static Fix64 Sqrt(Fix64 number) {
            return Fix64.Sqrt(number);
        }
        #endregion

        /// <summary>
        /// Gets the maximum number of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>Returns the largest value.</returns>
        /// <summary>取两个数中的较大值。</summary>
        /// <param name="val1">第一个值。</param>
        /// <param name="val2">第二个值。</param>
        /// <returns>较大值。</returns>
        #region public static FP Max(FP val1, FP val2)
        public static Fix64 Max(Fix64 val1, Fix64 val2) {
            return (val1 > val2) ? val1 : val2;
        }
        #endregion

        /// <summary>
        /// Gets the minimum number of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>Returns the smallest value.</returns>
        /// <summary>取两个数中的较小值。</summary>
        /// <param name="val1">第一个值。</param>
        /// <param name="val2">第二个值。</param>
        /// <returns>较小值。</returns>
        #region public static FP Min(FP val1, FP val2)
        public static Fix64 Min(Fix64 val1, Fix64 val2) {
            return (val1 < val2) ? val1 : val2;
        }
        #endregion

        /// <summary>
        /// Gets the maximum number of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>Returns the largest value.</returns>
        /// <summary>取三个数中的较大值。</summary>
        /// <param name="val1">第一个值。</param>
        /// <param name="val2">第二个值。</param>
        /// <param name="val3">第三个值。</param>
        /// <returns>较大值。</returns>
        #region public static FP Max(FP val1, FP val2,FP val3)
        public static Fix64 Max(Fix64 val1, Fix64 val2, Fix64 val3) {
            Fix64 max12 = (val1 > val2) ? val1 : val2;
            return (max12 > val3) ? max12 : val3;
        }
        #endregion

        /// <summary>
        /// Returns a number which is within [min,max]
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        /// <summary>将数值限制在 [min, max] 范围内。</summary>
        /// <param name="value">要限制的值。</param>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        /// <returns>限制后的值。</returns>
        #region public static FP Clamp(FP value, FP min, FP max)
        public static Fix64 Clamp(Fix64 value, Fix64 min, Fix64 max) {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }
        #endregion

        /// <summary>
        /// Changes every sign of the matrix entry to '+'
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="result">The absolute matrix.</param>
        /// <summary>将矩阵每个元素取绝对值。</summary>
        /// <param name="matrix">源矩阵。</param>
        /// <param name="result">绝对值后的矩阵。</param>
        #region public static void Absolute(ref JMatrix matrix,out JMatrix result)
        public static void Absolute(ref FPMatrix matrix, out FPMatrix result) {
            result.M11 = Fix64.Abs(matrix.M11);
            result.M12 = Fix64.Abs(matrix.M12);
            result.M13 = Fix64.Abs(matrix.M13);
            result.M21 = Fix64.Abs(matrix.M21);
            result.M22 = Fix64.Abs(matrix.M22);
            result.M23 = Fix64.Abs(matrix.M23);
            result.M31 = Fix64.Abs(matrix.M31);
            result.M32 = Fix64.Abs(matrix.M32);
            result.M33 = Fix64.Abs(matrix.M33);
        }
        #endregion

        /// <summary>
        /// Returns the sine of value.
        /// </summary>
        /// <summary>计算正弦值。</summary>
        /// <param name="value">弧度角。</param>
        /// <returns>正弦值。</returns>
        public static Fix64 Sin(Fix64 value) {
            return Fix64.Sin(value);
        }

        /// <summary>
        /// Returns the cosine of value.
        /// </summary>
        /// <summary>计算余弦值。</summary>
        /// <param name="value">弧度角。</param>
        /// <returns>余弦值。</returns>
        public static Fix64 Cos(Fix64 value) {
            return Fix64.Cos(value);
        }

        /// <summary>
        /// Returns the tan of value.
        /// </summary>
        /// <summary>计算正切值。</summary>
        /// <param name="value">弧度角。</param>
        /// <returns>正切值。</returns>
        public static Fix64 Tan(Fix64 value) {
            return Fix64.Tan(value);
        }

        /// <summary>
        /// Returns the arc sine of value.
        /// </summary>
        /// <summary>计算反正弦值。</summary>
        /// <param name="value">正弦值。</param>
        /// <returns>弧度角。</returns>
        public static Fix64 Asin(Fix64 value) {
            return Fix64.Asin(value);
        }

        /// <summary>
        /// Returns the arc cosine of value.
        /// </summary>
        /// <summary>计算反余弦值。</summary>
        /// <param name="value">余弦值。</param>
        /// <returns>弧度角。</returns>
        public static Fix64 Acos(Fix64 value) {
            return Fix64.Acos(value);
        }

        /// <summary>
        /// Returns the arc tan of value.
        /// </summary>
        /// <summary>计算反正切值。</summary>
        /// <param name="value">正切值。</param>
        /// <returns>弧度角。</returns>
        public static Fix64 Atan(Fix64 value) {
            return Fix64.Atan(value);
        }

        /// <summary>
        /// Returns the arc tan of coordinates x-y.
        /// </summary>
        /// <summary>计算坐标 (x, y) 的反正切值（四象限）。</summary>
        /// <param name="y">Y 坐标。</param>
        /// <param name="x">X 坐标。</param>
        /// <returns>弧度角。</returns>
        public static Fix64 Atan2(Fix64 y, Fix64 x) {
            return Fix64.Atan2(y, x);
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        /// <summary>向下取整。</summary>
        /// <param name="value">目标数。</param>
        /// <returns>向下取整结果。</returns>
        public static Fix64 Floor(Fix64 value) {
            return Fix64.Floor(value);
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        /// <summary>向上取整。</summary>
        /// <param name="value">目标数。</param>
        /// <returns>向上取整结果。</returns>
        public static Fix64 Ceiling(Fix64 value) {
            return value;
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        /// <summary>四舍五入到最近整数（中值舍入到偶数）。</summary>
        /// <param name="value">目标数。</param>
        /// <returns>四舍五入结果。</returns>
        public static Fix64 Round(Fix64 value) {
            return Fix64.Round(value);
        }

        /// <summary>
        /// Returns a number indicating the sign of a Fix64 number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        /// <summary>获取符号（正 1 / 零 0 / 负 -1）。</summary>
        /// <param name="value">目标数。</param>
        /// <returns>符号值。</returns>
        public static int Sign(Fix64 value) {
            return Fix64.Sign(value);
        }

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// Note: Abs(Fix64.MinValue) == Fix64.MaxValue.
        /// </summary>
        /// <summary>取绝对值（Abs(MinValue) 定义为 MaxValue）。</summary>
        /// <param name="value">目标数。</param>
        /// <returns>绝对值。</returns>
        public static Fix64 Abs(Fix64 value) {
            return Fix64.Abs(value);                
        }

        /// <summary>
        /// 重心坐标插值：value1 + (value2-value1)*amount1 + (value3-value1)*amount2。
        /// </summary>
        /// <param name="value1">顶点值1。</param>
        /// <param name="value2">顶点值2。</param>
        /// <param name="value3">顶点值3。</param>
        /// <param name="amount1">权重1。</param>
        /// <param name="amount2">权重2。</param>
        /// <returns>插值结果。</returns>
        public static Fix64 Barycentric(Fix64 value1, Fix64 value2, Fix64 value3, Fix64 amount1, Fix64 amount2) {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        /// <summary>
        /// Catmull-Rom 样条插值。
        /// </summary>
        /// <param name="value1">控制点1。</param>
        /// <param name="value2">控制点2。</param>
        /// <param name="value3">控制点3。</param>
        /// <param name="value4">控制点4。</param>
        /// <param name="amount">插值参数（0~1）。</param>
        /// <returns>插值结果。</returns>
        public static Fix64 CatmullRom(Fix64 value1, Fix64 value2, Fix64 value3, Fix64 value4, Fix64 amount) {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using FPs not to lose precission
            Fix64 amountSquared = amount * amount;
            Fix64 amountCubed = amountSquared * amount;
            return (Fix64)(0.5 * (2.0 * value2 +
                                 (value3 - value1) * amount +
                                 (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                                 (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        /// <summary>
        /// 计算两个数的距离（绝对值差）。
        /// </summary>
        /// <param name="value1">值1。</param>
        /// <param name="value2">值2。</param>
        /// <returns>距离。</returns>
        public static Fix64 Distance(Fix64 value1, Fix64 value2) {
            return Fix64.Abs(value1 - value2);
        }

        /// <summary>
        /// Hermite 样条插值（带切线）。
        /// </summary>
        /// <param name="value1">起点值。</param>
        /// <param name="tangent1">起点切线。</param>
        /// <param name="value2">终点值。</param>
        /// <param name="tangent2">终点切线。</param>
        /// <param name="amount">插值参数（0~1）。</param>
        /// <returns>插值结果。</returns>
        public static Fix64 Hermite(Fix64 value1, Fix64 tangent1, Fix64 value2, Fix64 tangent2, Fix64 amount) {
            // All transformed to FP not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            Fix64 v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            Fix64 sCubed = s * s * s;
            Fix64 sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                         (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                         t1 * s +
                         v1;
            return (Fix64)result;
        }

        /// <summary>
        /// 线性插值：value1 + (value2 - value1) * amount。
        /// </summary>
        /// <param name="value1">起点值。</param>
        /// <param name="value2">终点值。</param>
        /// <param name="amount">插值参数（0~1）。</param>
        /// <returns>插值结果。</returns>
        public static Fix64 Lerp(Fix64 value1, Fix64 value2, Fix64 amount) {
            return value1 + (value2 - value1) * amount;
        }

        /// <summary>
        /// 平滑插值（SmoothStep）：使用 Hermite 插值实现平滑过渡。
        /// </summary>
        /// <param name="value1">起点值。</param>
        /// <param name="value2">终点值。</param>
        /// <param name="amount">插值参数（自动夹取到 0~1）。</param>
        /// <returns>平滑插值结果。</returns>
        public static Fix64 SmoothStep(Fix64 value1, Fix64 value2, Fix64 amount) {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            Fix64 result = Clamp(amount, 0f, 1f);
            result = Hermite(value1, 0f, value2, 0f, result);
            return result;
        }

    }
}

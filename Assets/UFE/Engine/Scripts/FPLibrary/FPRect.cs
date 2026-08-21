using System;
using UnityEngine;

namespace FPLibrary
{
    /// <summary>
    /// A simple fixed point rect structure.
    /// </summary>
    /// <summary>
    /// 定点矩形（FPRect）。
    /// <para>用途：以定点数表示矩形（含四角点与 x/y/width/height/xMax/yMax 属性），</para>
    /// <para>提供碰撞相交检测（Intersects）与点到矩形最近距离（DistanceToPoint），用于判定盒的矩形碰撞。</para>
    /// </summary>
    [Serializable]
    public struct FPRect
    {
        /// <summary>右上角点。</summary>
        public FPVector topRight;
        /// <summary>左上角点。</summary>
        public FPVector topLeft;
        /// <summary>右下角点。</summary>
        public FPVector bottomRight;
        /// <summary>左下角点。</summary>
        public FPVector bottomLeft;
        /// <summary>X 坐标。</summary>
        public Fix64 x;
        /// <summary>Y 坐标。</summary>
        public Fix64 y;
        /// <summary>宽度。</summary>
        public Fix64 width;
        /// <summary>高度。</summary>
        public Fix64 height;
        /// <summary>最大 X（= x + width）。</summary>
        public Fix64 xMax;
        /// <summary>最大 Y（= y + height）。</summary>
        public Fix64 yMax;

        /// <summary>
        /// 从 Unity Rect 构造定点矩形。
        /// </summary>
        /// <param name="rect">Unity 矩形。</param>
        public FPRect(Rect rect)
        {
            this.topLeft = new FPVector(rect.x, rect.y, 0);
            this.topRight = new FPVector(rect.xMax, rect.y, 0);
            this.bottomLeft = new FPVector(rect.x, rect.yMax, 0);
            this.bottomRight = new FPVector(rect.xMax, rect.yMax, 0);
            this.x = rect.x;
            this.y = rect.y;
            this.width = rect.width;
            this.height = rect.height;
            this.xMax = rect.xMax;
            this.yMax = rect.yMax;
        }

        /// <summary>
        /// 移动矩形到指定位置并刷新四角点。
        /// </summary>
        /// <param name="fpVector">新的左上角位置。</param>
        public void MoveTo(FPVector fpVector)
        {
            this.x = fpVector.x;
            this.y = fpVector.y;
            RefreshPoints();
        }

        /// <summary>
        /// 根据 x/y/width/height 刷新四角点与 xMax/yMax。
        /// </summary>
        public void RefreshPoints()
        {
            this.xMax = this.x + this.width;
            this.yMax = this.y + this.height;
            this.topLeft = new FPVector(this.x, this.y, 0);
            this.topRight = new FPVector(this.xMax, this.y, 0);
            this.bottomLeft = new FPVector(this.x, this.yMax, 0);
            this.bottomRight = new FPVector(this.xMax, this.yMax, 0);
        }
        
        /// <summary>
        /// 判断两个矩形是否相交。
        /// </summary>
        /// <param name="rect">另一个矩形。</param>
        /// <returns>相交返回 true。</returns>
        public bool Intersects(FPRect rect)
        {
            return rect.topLeft.x < this.topRight.x &&
                   this.topLeft.x < rect.topRight.x &&
                   rect.topLeft.y < this.bottomLeft.y &&
                   this.topLeft.y < rect.bottomLeft.y;
        }

        /// <summary>
        /// 计算点到矩形的最短距离（点在矩形内返回 0，外部按九宫格区域计算到边或角点的距离）。
        /// </summary>
        /// <param name="point">目标点。</param>
        /// <returns>最短距离。</returns>
        public Fix64 DistanceToPoint(FPVector point)
        {
            Fix64 xMax = this.topRight.x;
            Fix64 xMin = this.topLeft.x;
            Fix64 yMax = this.bottomRight.y;
            Fix64 yMin = this.topRight.y;

            if (point.x < xMin)
            { // Region I, VIII, or VII
                if (point.y < yMin)
                { // I
                    FPVector diff = point - new FPVector(xMin, yMin, 0);
                    return diff.magnitude;
                }
                else if (point.y > this.bottomRight.y)
                { // VII
                    FPVector diff = point - new FPVector(xMin, yMax, 0);
                    return diff.magnitude;
                }
                else
                { // VIII
                    return xMin - point.x;
                }
            }
            else if (point.x > xMax)
            { // Region III, IV, or V
                if (point.y < yMin)
                { // III
                    FPVector diff = point - new FPVector(xMax, yMin, 0);
                    return diff.magnitude;
                }
                else if (point.y > yMax)
                { // V
                    FPVector diff = point - new FPVector(xMax, yMax, 0);
                    return diff.magnitude;
                }
                else
                { // IV
                    return point.x - xMax;
                }
            }
            else
            { // Region II, IX, or VI
                if (point.y < yMin)
                { // II
                    return yMin - point.y;
                }
                else if (point.y > yMax)
                { // VI
                    return point.y - yMax;
                }
                else
                { // IX
                    return 0;
                }
            }
        }
    }
}

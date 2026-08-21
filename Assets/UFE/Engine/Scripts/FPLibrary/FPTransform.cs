using UnityEngine;

namespace FPLibrary
{
    /// <summary>
    /// 定点变换（FPTransform）。
    /// <para>用途：以定点数（FPVector/FPQuaternion）保存物体变换（位置/旋转/缩放）的组件，</para>
    /// <para>提供 LookAt/Translate/Rotate/RotateAround 及前/右/上方向、欧拉角等属性，</para>
    /// <para>供帧同步中确定性移动/转向使用，可将定点变换同步到 Unity Transform。</para>
    /// </summary>
    public class FPTransform : MonoBehaviour
    {
        /// <summary>位置（内部存储）。</summary>
        private FPVector _position;
        /// <summary>位置（定点数）。</summary>
        public FPVector position {
            get {
				return _position;
            }
            set {
                _position = value;
            }
        }
        
        /// <summary>旋转（内部存储）。</summary>
        private FPQuaternion _rotation;
        /// <summary>旋转（定点四元数）。</summary>
        public FPQuaternion rotation {
            get {
                return _rotation;
            }
            set {
                _rotation = value;
            }
        }

        /// <summary>缩放（内部存储）。</summary>
        private FPVector _scale;
        /// <summary>缩放（定点数）。</summary>
        public FPVector scale {
            get {
                return _scale;
            }
            set {
                _scale = value;
            }
        }
        
        /// <summary>
        /// 注视另一个变换的位置。
        /// </summary>
        /// <param name="other">目标变换。</param>
        public void LookAt(FPTransform other) {
            LookAt(other.position);
        }
        
        /// <summary>
        /// 注视指定目标位置（旋转朝向目标）。
        /// </summary>
        /// <param name="target">目标位置。</param>
        public void LookAt(FPVector target) {
            this.rotation = FPQuaternion.CreateFromMatrix(FPMatrix.CreateFromLookAt(position, target));
        }
        
        /// <summary>
        /// 平移（默认相对自身坐标系）。
        /// </summary>
        /// <param name="x">X 位移。</param>
        /// <param name="y">Y 位移。</param>
        /// <param name="z">Z 位移。</param>
        public void Translate(Fix64 x, Fix64 y, Fix64 z) {
            Translate(x, y, z, Space.Self);
        }
        
        /// <summary>
        /// 平移（按相对空间：Self=自身旋转方向，World=世界坐标）。
        /// </summary>
        /// <param name="x">X 位移。</param>
        /// <param name="y">Y 位移。</param>
        /// <param name="z">Z 位移。</param>
        /// <param name="relativeTo">相对空间。</param>
        public void Translate(Fix64 x, Fix64 y, Fix64 z, Space relativeTo) {
            Translate(new FPVector(x, y, z), relativeTo);
        }
        
        /// <summary>
        /// 平移（相对指定变换的坐标系）。
        /// </summary>
        /// <param name="x">X 位移。</param>
        /// <param name="y">Y 位移。</param>
        /// <param name="z">Z 位移。</param>
        /// <param name="relativeTo">参考变换。</param>
        public void Translate(Fix64 x, Fix64 y, Fix64 z, FPTransform relativeTo) {
            Translate(new FPVector(x, y, z), relativeTo);
        }
        
        /// <summary>
        /// 平移（默认相对自身坐标系）。
        /// </summary>
        /// <param name="translation">位移向量。</param>
        public void Translate(FPVector translation) {
            Translate(translation, Space.Self);
        }
        
        /// <summary>
        /// 平移（按相对空间）。
        /// </summary>
        /// <param name="translation">位移向量。</param>
        /// <param name="relativeTo">相对空间。</param>
        public void Translate(FPVector translation, Space relativeTo) {
            if (relativeTo == Space.Self) {
                Translate(translation, this);
            } else {
                this.position += translation;
            }
        }
        
        /// <summary>
        /// 平移（相对指定变换：将位移按参考变换的旋转转换到世界坐标）。
        /// </summary>
        /// <param name="translation">位移向量。</param>
        /// <param name="relativeTo">参考变换。</param>
        public void Translate(FPVector translation, FPTransform relativeTo) {
            this.position += FPVector.Transform(translation, FPMatrix.CreateFromQuaternion(relativeTo.rotation));
        }
        
        /// <summary>
        /// 绕世界空间中的点与轴旋转（位移加自转）。
        /// </summary>
        /// <param name="point">旋转中心点。</param>
        /// <param name="axis">旋转轴。</param>
        /// <param name="angle">旋转角度（度）。</param>
        public void RotateAround(FPVector point, FPVector axis, Fix64 angle) {
            FPVector vector = this.position;
            FPVector vector2 = vector - point;
            vector2 = FPVector.Transform(vector2, FPMatrix.AngleAxis(angle * Fix64.Deg2Rad, axis));
            vector = point + vector2;
            this.position = vector;

            Rotate(axis, angle);
        }
        
        /// <summary>
        /// 绕指定轴旋转（自转）。
        /// </summary>
        /// <param name="axis">旋转轴。</param>
        /// <param name="angle">旋转角度（度）。</param>
        public void RotateAround(FPVector axis, Fix64 angle) {
            Rotate(axis, angle);
        }
        
        /// <summary>
        /// 按欧拉角旋转（默认相对自身坐标系）。
        /// </summary>
        /// <param name="xAngle">X 轴旋转角度。</param>
        /// <param name="yAngle">Y 轴旋转角度。</param>
        /// <param name="zAngle">Z 轴旋转角度。</param>
        public void Rotate(Fix64 xAngle, Fix64 yAngle, Fix64 zAngle) {
            Rotate(new FPVector(xAngle, yAngle, zAngle), Space.Self);
        }
        
        /// <summary>
        /// 按欧拉角旋转（指定相对空间）。
        /// </summary>
        /// <param name="xAngle">X 轴旋转角度。</param>
        /// <param name="yAngle">Y 轴旋转角度。</param>
        /// <param name="zAngle">Z 轴旋转角度。</param>
        /// <param name="relativeTo">相对空间。</param>
        public void Rotate(Fix64 xAngle, Fix64 yAngle, Fix64 zAngle, Space relativeTo) {
            Rotate(new FPVector(xAngle, yAngle, zAngle), relativeTo);
        }
        
        /// <summary>
        /// 按欧拉角向量旋转（默认相对自身坐标系）。
        /// </summary>
        /// <param name="eulerAngles">欧拉角向量。</param>
        public void Rotate(FPVector eulerAngles) {
            Rotate(eulerAngles, Space.Self);
        }
        
        /// <summary>
        /// 绕指定轴旋转（默认相对自身坐标系）。
        /// </summary>
        /// <param name="axis">旋转轴。</param>
        /// <param name="angle">旋转角度（度）。</param>
        public void Rotate(FPVector axis, Fix64 angle) {
            Rotate(axis, angle, Space.Self);
        }
        
        /// <summary>
        /// 绕指定轴旋转（指定相对空间：Self 后乘自身旋转，World 前乘自身旋转）。
        /// </summary>
        /// <param name="axis">旋转轴。</param>
        /// <param name="angle">旋转角度（度）。</param>
        /// <param name="relativeTo">相对空间。</param>
        public void Rotate(FPVector axis, Fix64 angle, Space relativeTo) {
            FPQuaternion result = FPQuaternion.identity;

            if (relativeTo == Space.Self) {
                result = this.rotation * FPQuaternion.AngleAxis(angle, axis);
            } else {
                result = FPQuaternion.AngleAxis(angle, axis) * this.rotation;
            }

            result.Normalize();
            this.rotation = result;
        }
        
        /// <summary>
        /// 按欧拉角向量旋转（指定相对空间）。
        /// </summary>
        /// <param name="eulerAngles">欧拉角向量。</param>
        /// <param name="relativeTo">相对空间。</param>
        public void Rotate(FPVector eulerAngles, Space relativeTo) {
            FPQuaternion result = FPQuaternion.identity;

            if (relativeTo == Space.Self) {
                result = this.rotation * FPQuaternion.Euler(eulerAngles);
            } else {
                result = FPQuaternion.Euler(eulerAngles) * this.rotation;
            }

            result.Normalize();
            this.rotation = result;
        }
        
        /// <summary>
        /// 前方向量（按当前旋转变换 Z 轴正方向）。
        /// </summary>
        public FPVector forward {
            get {
                return FPVector.Transform(FPVector.forward, FPMatrix.CreateFromQuaternion(rotation));
            }
        }
        
        /// <summary>
        /// 右方向量（按当前旋转变换 X 轴正方向）。
        /// </summary>
        public FPVector right {
            get {
                return FPVector.Transform(FPVector.right, FPMatrix.CreateFromQuaternion(rotation));
            }
        }
        
        /// <summary>
        /// 上方向量（按当前旋转变换 Y 轴正方向）。
        /// </summary>
        public FPVector up {
            get {
                return FPVector.Transform(FPVector.up, FPMatrix.CreateFromQuaternion(rotation));
            }
        }
        
        /// <summary>
        /// 欧拉角（度）。
        /// </summary>
        public FPVector eulerAngles {
            get {
                return rotation.eulerAngles;
            }
        }

        /// <summary>
        /// 将定点位置同步到 Unity Transform。
        /// </summary>
        /// <param name="transform">目标 Unity Transform。</param>
        public void UpdateTransform(Transform transform) {
            transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);
            //transform.rotation = new Quaternion((float)rotation.x, (float)rotation.y, (float)rotation.z, (float)rotation.w);
            //transform.localScale = new Vector3((float)scale.x, (float)scale.y, (float)scale.z);
        }

    }

}

using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 第三人称摄像机：跟随目标、右键旋转、滚轮缩放。
    /// 参考实现（第 2 步）：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("跟随目标")]
        public Transform target;

        [Header("视角参数")]
        public float distance = 6f;      // 相机与目标的距离
        public float rotateSpeed = 3f;   // 旋转灵敏度
        public float pitchMin = -30f;    // 俯仰角下限（防止转到地面以下）
        public float pitchMax = 60f;     // 俯仰角上限

        float yaw;   // 水平角（绕 Y 轴）
        float pitch; // 俯仰角（绕 X 轴）

        void Start()
        {
            if (target == null) return;

            // 根据相机与目标的初始位置差反算 yaw/pitch 与 distance，
            // 避免游戏一开始相机角度跳变
            Vector3 diff = target.position - transform.position;
            distance = Mathf.Max(diff.magnitude, 0.1f);
            yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
            float flat = Mathf.Sqrt(diff.x * diff.x + diff.z * diff.z);
            pitch = Mathf.Atan2(diff.y, flat) * Mathf.Rad2Deg;
        }

        void Update()
        {
            // 按住鼠标右键拖动旋转视角（动作游戏习惯）
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * rotateSpeed;
                pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }

            // 鼠标滚轮缩放距离
            distance -= Input.GetAxis("Mouse ScrollWheel") * 2f;
            distance = Mathf.Clamp(distance, 3f, 15f);
        }

        void LateUpdate()
        {
            if (target == null) return;

            // 1) 用欧拉角构造旋转  2) 算出目标后方的偏移点  3) 看向目标
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position + rot * Vector3.forward * (-distance);
            transform.rotation = Quaternion.LookRotation(target.position - transform.position);
        }
    }
}

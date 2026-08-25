using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 玩家移动控制（第 3 步）：WASD 移动、平滑转向、基于相机朝向。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float rotateSpeed = 10f;
        public float gravity = 9.8f;

        CharacterController cc;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        void Update()
        {
            float h = Input.GetAxisRaw("Horizontal"); // A/D 或 左右方向键
            float v = Input.GetAxisRaw("Vertical");   // W/S 或 上下方向键

            // 把相机的前/右方向投影到水平面，作为移动参考系
            Transform cam = Camera.main.transform;
            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            Vector3 moveDir = camForward * v + camRight * h;
            if (moveDir.sqrMagnitude > 1f) moveDir = moveDir.normalized; // 防止斜向超速

            if (moveDir.sqrMagnitude > 0.01f)
            {
                // 平滑转向移动方向
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            // 重力 + 位移（注意必须乘 Time.deltaTime）
            cc.Move((moveDir * moveSpeed + Vector3.down * gravity) * Time.deltaTime);
        }
    }
}

using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 玩家移动控制（第 3 步）：WASD 移动、平滑转向、基于相机朝向。
    /// 动画计划第 1 步新增：把移动状态写入 Animator 的 Speed 参数，驱动 Idle/Walk 切换与 BlendTree 混合。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float rotateSpeed = 10f;
        public float gravity = 9.8f;

        // ---- 动画计划第 1 步新增 ----
        // 玩家模型的 Animator（挂的是第 0 步生成的 PlayerAnimator.controller）
        // Awake 会自动取同物体上的 Animator；如果脚本和 Animator 不在同一物体，请在 Inspector 手动拖入
        public Animator anim;

        CharacterController cc;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (anim == null) anim = GetComponent<Animator>(); // 兜底：同物体上自动获取
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

            // ---- 动画计划第 1 步新增：把移动状态写入动画参数 Speed ----
            // 有输入 → Speed=1（走路）；无输入 → Speed=0（站立）
            // 控制器里：Speed>0.1 触发 Idle→Walk、Speed<0.1 触发 Walk→Idle；
            // Walk 状态内部是 1D BlendTree（Speed 0→混合 Idle 片段，1→混合 Walk 片段）
            float speedParam = moveDir.sqrMagnitude > 0.01f ? 1f : 0f;
            if (anim != null) anim.SetFloat("Speed", speedParam);
        }
    }
}

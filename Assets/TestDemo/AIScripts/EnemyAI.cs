using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 简单敌人 AI（第 8 步）：最简状态机 Idle / Chase / Attack。
    /// 玩家进入视野范围后追击，贴近后持续攻击。
    /// 动画计划第 4 步新增：AI 状态机与动画同步——Chase 走路、Idle/Attack 站立、攻击播 Attack 动画。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState { Idle, Chase, Attack }

        [Header("AI 参数")]
        public float detectRange = 8f;     // 发现玩家距离
        public float attackRange = 1.8f;   // 攻击距离
        public float moveSpeed = 3f;
        public float attackInterval = 1.2f; // 攻击间隔
        public int attackDamage = 8;

        public Transform player;           // 玩家引用（运行时自动查找）

        AIState state = AIState.Idle;
        float lastAttackTime;
        CharacterController cc;

        // ---- 动画计划第 4 步新增 ----
        // 敌人模型的 Animator（挂的是第 0 步生成的 EnemyAnimator.controller）
        // Awake 自动取同物体上的；不在同一物体时在 Inspector 手动拖
        public Animator anim;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (anim == null) anim = GetComponent<Animator>(); // 兜底：同物体上自动获取
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Update()
        {
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);
            switch (state)
            {
                case AIState.Idle:
                    // ---- 动画计划第 4 步新增：站立动画 ----
                    if (anim != null) anim.SetFloat("Speed", 0f);
                    if (dist <= detectRange) state = AIState.Chase;
                    break;

                case AIState.Chase:
                    FacePlayer();
                    cc.Move(transform.forward * moveSpeed * Time.deltaTime);
                    // ---- 动画计划第 4 步新增：走路动画（Speed=1 让 BlendTree 混合到走路）----
                    if (anim != null) anim.SetFloat("Speed", 1f);
                    if (dist <= attackRange) state = AIState.Attack;
                    else if (dist > detectRange * 1.5f) state = AIState.Idle;
                    break;

                case AIState.Attack:
                    FacePlayer();
                    // ---- 动画计划第 4 步新增：站立动画 ----
                    if (anim != null) anim.SetFloat("Speed", 0f);
                    if (Time.time - lastAttackTime >= attackInterval)
                    {
                        lastAttackTime = Time.time;
                        // ---- 动画计划第 4 步新增：攻击动画（与玩家共用 Attack 状态）----
                        if (anim != null) anim.SetTrigger("Attack");
                        Health ph = player.GetComponent<Health>();
                        if (ph != null)
                            ph.TakeDamage(attackDamage, transform.position - player.position);
                    }
                    if (dist > attackRange * 1.2f) state = AIState.Chase;
                    break;
            }
        }

        void FacePlayer()
        {
            // 只旋转 Y 轴，避免敌人倾斜
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }
}

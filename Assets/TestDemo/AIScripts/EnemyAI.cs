using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 简单敌人 AI（第 8 步）：最简状态机 Idle / Chase / Attack。
    /// 玩家进入视野范围后追击，贴近后持续攻击。
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

        void Awake()
        {
            cc = GetComponent<CharacterController>();
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
                    if (dist <= detectRange) state = AIState.Chase;
                    break;

                case AIState.Chase:
                    FacePlayer();
                    cc.Move(transform.forward * moveSpeed * Time.deltaTime);
                    if (dist <= attackRange) state = AIState.Attack;
                    else if (dist > detectRange * 1.5f) state = AIState.Idle;
                    break;

                case AIState.Attack:
                    FacePlayer();
                    if (Time.time - lastAttackTime >= attackInterval)
                    {
                        lastAttackTime = Time.time;
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

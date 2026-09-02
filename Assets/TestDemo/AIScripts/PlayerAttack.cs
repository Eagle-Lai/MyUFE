using System.Collections;
using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 近战攻击（第 4 + 6 步）：J 键触发，前冲模拟挥拳，
    /// 并在面前生成短暂存在的判定球（AttackHit）造成伤害。
    /// 动画计划第 2 步新增：攻击时 SetTrigger("Attack") 播放攻击动画。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        public float attackCooldown = 0.8f; // 攻击间隔（秒）
        public float lungeSpeed = 12f;      // 前冲速度
        public float lungeTime = 0.15f;     // 前冲持续时间
        public int attackDamage = 10;       // 攻击力

        float lastAttackTime = -999f;       // 初始化为足够小，保证第一次能攻击
        CharacterController cc;

        // ---- 动画计划第 2 步新增 ----
        // 玩家模型的 Animator（Awake 自动取同物体上的；不在同一物体时在 Inspector 手动拖）
        public Animator anim;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (anim == null) anim = GetComponent<Animator>(); // 兜底：同物体上自动获取
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.J)) TryAttack();
        }

        void TryAttack()
        {
            // 冷却判断：时间差不够则直接返回
            if (Time.time - lastAttackTime < attackCooldown)
            {
                Debug.Log("攻击冷却中");
                return;
            }
            lastAttackTime = Time.time;

            // ---- 动画计划第 2 步新增：触发攻击动画 ----
            // Trigger 是"一次性信号"：置位后控制器里 Any State→Attack 的转换立即满足，
            // Attack 状态播放 E_Stand_N1，播到 90% 自动回 Idle/Walk（第 0 步搭好的转换）
            // 注意：Trigger 被转换消费后自动清零，不需要手动 Reset（主动重置用 anim.ResetTrigger）
            if (anim != null) anim.SetTrigger("Attack");

            StartCoroutine(AttackRoutine());
        }

        IEnumerator AttackRoutine()
        {
            // 用一小段前冲模拟挥拳动作
            float end = Time.time + lungeTime;
            while (Time.time < end)
            {
                cc.Move(transform.forward * lungeSpeed * Time.deltaTime);
                yield return null; // 等待下一帧
            }

            // 第 6 步内容：前冲结束时在面前生成攻击判定球（届时取消注释）
            GameObject go = new GameObject("AttackHit");
            go.transform.position = transform.position + transform.forward * 1f;
            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.6f;
            AttackHit hit = go.AddComponent<AttackHit>();
            hit.damage = attackDamage;
            hit.owner = gameObject;
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 生命值系统（第 5 + 9 步）：受伤/治疗/死亡，事件通知外部，
    /// 含受击反馈（击退 + 闪红）。
    /// 动画计划第 5 步新增：受伤播 Hit 动画、死亡播 Death 动画（致死时只播 Death 避免触发竞争）。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class Health : MonoBehaviour
    {
        public int maxHealth = 100;
        [SerializeField] int currentHealth;

        public int CurrentHealth { get { return currentHealth; } }
        public bool IsDead { get { return currentHealth <= 0; } }

        public event Action OnDamaged; // 受伤/治疗时触发（UI 血条订阅）
        public event Action OnDeath;   // 死亡时触发（胜负判定订阅）

        // ---- 动画计划第 5 步新增 ----
        // 本物体模型的 Animator（Awake 自动取同物体上的；不在同一物体时在 Inspector 手动拖）
        public Animator anim;

        void Awake()
        {
            if (anim == null) anim = GetComponent<Animator>(); // 兜底：同物体上自动获取
        }

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return; // 死亡后不再受伤
            currentHealth = Mathf.Max(0, currentHealth - damage);

            // ---- 动画计划第 5 步新增：受击动画 ----
            // 顺序关键：致死时不播 Hit（走 Die 播 Death），避免 Hit/Death 两个触发竞争
            if (!IsDead && anim != null) anim.SetTrigger("Hit");

            if (OnDamaged != null) OnDamaged(); // 通知订阅者
            if (IsDead) Die();
        }

        // 第 9 步内容：带击退方向的伤害重载（届时启用）
        public void TakeDamage(int damage, Vector3 hitDirection)
        {
            if (IsDead) return;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if (hitDirection.sqrMagnitude > 0.01f)
            {
                CharacterController cc = GetComponent<CharacterController>();
                if (cc != null) cc.Move(hitDirection.normalized * 0.3f);
            }
            StartCoroutine(FlashRed());
            // ---- 动画计划第 5 步新增：受击动画（致死时不播，交给 Die 的 Death）----
            if (!IsDead && anim != null) anim.SetTrigger("Hit");
            if (OnDamaged != null) OnDamaged();
            if (IsDead) Die();
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            if (OnDamaged != null) OnDamaged();
        }

        /// <summary>重设回满血并恢复可行动（供 GameManager 重置战斗使用）</summary>
        public void Restore()
        {
            currentHealth = maxHealth;
            enabled = true;
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;

            // ---- 动画计划第 6 步新增：重置战斗时把动画拉回站立 ----
            // Play("Idle") 是"直接跳转"（不受转换条件约束），
            // 能把停在 Death 躺尸状态的模型强制拉回 Idle
            if (anim != null) anim.Play("Idle", 0, 0f);
        }

        void Die()
        {
            // ---- 动画计划第 5 步新增：死亡动画 ----
            // Any State→Death（第 0 步已搭），播放 E_Basic_Fall_Back / FallDown，无出口永久停留
            if (anim != null) anim.SetTrigger("Death");
            if (OnDeath != null) OnDeath();
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // 停止移动
            enabled = false;                    // 停止本组件
        }

        // 第 9 步内容：闪红协程（届时启用）
        IEnumerator FlashRed()
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r == null) yield break;
            Color original = r.material.color; // material：实例化，不影响其他物体
            r.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            r.material.color = original;
        }
    }
}

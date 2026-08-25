using System;
using System.Collections;
using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 生命值系统（第 5 + 9 步）：受伤/治疗/死亡，事件通知外部，
    /// 含受击反馈（击退 + 闪红）。
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

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return; // 死亡后不再受伤
            currentHealth = Mathf.Max(0, currentHealth - damage);

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
        }

        void Die()
        {
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

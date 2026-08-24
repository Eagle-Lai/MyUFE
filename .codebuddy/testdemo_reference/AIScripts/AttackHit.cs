using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 攻击判定球（第 6 步）：短暂存在，命中带 Health 的对象造成伤害。
    /// 由 PlayerAttack 在攻击时运行时动态创建，不需要手工摆放。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class AttackHit : MonoBehaviour
    {
        public int damage = 10;
        public float lifeTime = 0.1f;   // 判定球存在时间
        public GameObject owner;        // 攻击者，防止打到自己

        void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        void OnTriggerEnter(Collider other)
        {
            // 忽略攻击者自己
            if (owner != null && other.transform.IsChildOf(owner.transform)) return;

            Health h = other.GetComponentInParent<Health>();
            if (h != null)
            {
                Vector3 dir = other.transform.position - owner.transform.position;
                h.TakeDamage(damage, dir);
                // 命中特效：可在此实例化受击粒子（扩展路线⑤）
            }
        }
    }
}

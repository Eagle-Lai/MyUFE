using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 技能弹体（第 7 步）：直线飞行，命中带 Health 的对象造成伤害后销毁。
    /// 挂在技能弹体预制体（EnergyBall）上。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public int damage = 20;
        public float speed = 15f;
        public GameObject owner;

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform)) return;

            Health h = other.GetComponentInParent<Health>();
            if (h != null)
            {
                Vector3 dir = other.transform.position - transform.position;
                h.TakeDamage(damage, dir);
            }
            Destroy(gameObject); // 命中后消失
        }
    }
}

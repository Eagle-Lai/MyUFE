using System.Collections;
using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 近战攻击（第 4 + 6 步）：J 键触发，前冲模拟挥拳，
    /// 并在面前生成短暂存在的判定球（AttackHit）造成伤害。
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

        void Awake()
        {
            cc = GetComponent<CharacterController>();
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
            // GameObject go = new GameObject("AttackHit");
            // go.transform.position = transform.position + transform.forward * 1f;
            // SphereCollider sc = go.AddComponent<SphereCollider>();
            // sc.isTrigger = true;
            // sc.radius = 0.6f;
            // AttackHit hit = go.AddComponent<AttackHit>();
            // hit.damage = attackDamage;
            // hit.owner = gameObject;
        }
    }
}

using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 技能系统（第 7 步）：K 键释放，检查冷却后实例化弹体（Projectile）。
    /// 动画计划第 3 步新增：释放技能时 SetTrigger("Attack")，复用攻击动画状态。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class SkillController : MonoBehaviour
    {
        public Skill[] skills;              // Inspector 中配置
        public GameObject projectilePrefab; // 弹体预制体

        // ---- 动画计划第 3 步新增 ----
        // 玩家模型的 Animator（Awake 自动取同物体上的；不在同一物体时在 Inspector 手动拖）
        public Animator anim;

        void Awake()
        {
            if (anim == null) anim = GetComponent<Animator>(); // 兜底：同物体上自动获取
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) ReleaseSkill(0);
        }

        public void ReleaseSkill(int index) // 供 UI 按钮调用的公共接口
        {
            if (index < 0 || index >= skills.Length) return;

            Skill s = skills[index];
            float remain = s.cooldown - (Time.time - s.lastUsedTime);
            if (remain > 0f)
            {
                Debug.Log(s.name + " 冷却中，还需 " + Mathf.Ceil(remain) + " 秒");
                return;
            }
            s.lastUsedTime = Time.time;

            // ---- 动画计划第 3 步新增：释放技能也播攻击动画 ----
            // 复用控制器的 Attack 状态：技能与普攻共用同一个动画，
            // 因为触发的是同一个 Trigger 参数，控制器并不关心是谁置位的
            // （以后要做独立技能动作，只需在控制器加新状态和新 Trigger）
            if (anim != null) anim.SetTrigger("Attack");

            // 从角色前方 1 米、高度 1 米处发射
            Vector3 pos = transform.position + transform.forward * 1f + Vector3.up * 1f;
            GameObject go = Instantiate(projectilePrefab, pos, transform.rotation);
            Projectile p = go.GetComponent<Projectile>();
            if (p != null)
            {
                p.damage = s.damage;
                p.speed = s.speed;
                p.owner = gameObject;
            }
        }
    }
}

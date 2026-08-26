using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 技能系统（第 7 步）：K 键释放，检查冷却后实例化弹体（Projectile）。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class SkillController : MonoBehaviour
    {
        public Skill[] skills;              // Inspector 中配置
        public GameObject projectilePrefab; // 弹体预制体

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

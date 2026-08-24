using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// 技能数据类（第 7 步）：普通可序列化类，非组件。
    /// 在 SkillController 的 skills 数组里配置。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    [System.Serializable]
    public class Skill
    {
        public string name = "能量球";
        public int damage = 20;
        public float cooldown = 3f;   // 冷却（秒）
        public float speed = 15f;     // 弹体速度
        [HideInInspector] public float lastUsedTime = -999f; // 上次释放时间
    }
}

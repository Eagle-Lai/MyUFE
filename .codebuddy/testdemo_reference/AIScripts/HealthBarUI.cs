using UnityEngine;
using UnityEngine.UI;

namespace AIScripts
{
    /// <summary>
    /// 血条（第 10 步）：订阅目标 Health 的事件，实时刷新 Image.fillAmount。
    /// 挂在 Canvas 下血条对象（持有前景 Image 与目标 Health 引用）上。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        public Image fill;     // 前景条（fillAmount 会变化）
        public Health target; // 要显示的角色

        void OnEnable()
        {
            if (target != null) target.OnDamaged += Refresh;
        }

        void OnDisable()
        {
            if (target != null) target.OnDamaged -= Refresh;
        }

        void Start()
        {
            Refresh(); // 初始化
        }

        void Refresh()
        {
            if (fill != null && target != null)
                fill.fillAmount = (float)target.CurrentHealth / target.maxHealth;
        }
    }
}

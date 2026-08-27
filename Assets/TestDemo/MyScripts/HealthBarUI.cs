using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyScripts
{
    public class HealthBarUI : MonoBehaviour
    {
        public Image fill;
        public Health target;

        private void OnEnable()
        {
            if(target != null)
            {
                target.OnDamaged += Refresh;
            }
        }

        private void OnDisable()
        {
            if(target != null)
            {
                target.OnDamaged -= Refresh;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if(fill != null && target != null)
            {
                fill.fillAmount = (float)target.CurrentHealth / target.MaxHealth;
            }
        }
    }
}
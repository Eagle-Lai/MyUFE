using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    [SerializeField]
    public class SkillController : MonoBehaviour
    {
        [SerializeField]  
        public Skill[] skills;
        public GameObject projectilePrefab;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                ReleaseSkill(0);
            }
        }

        public void ReleaseSkill(int index)
        {
            if (index < 0 || index >= skills.Length) return;
            Skill skill = skills[index];

            float remain = skill.coolDown - (Time.time - skill.lastUsedTime);
            if(remain > 0)
            {
                return;
            }
            skill.lastUsedTime = Time.time;

            Vector3 pos = transform.position + transform.forward * 1f + Vector3.up * 1f;
            GameObject go = Instantiate(projectilePrefab, pos, transform.rotation);
            Projectile projectile = go.GetComponent<Projectile>();
            if(projectile != null)
            {
                projectile.damage = skill.damage;
                projectile.speed = skill.speed;
                projectile.owner = gameObject;
            }
        }
    }
}
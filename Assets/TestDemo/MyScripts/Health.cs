using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts
{
    public class Health : MonoBehaviour
    {
        public int MaxHealth = 1000;
        [SerializeField]
        int currentHealth;
        public int CurrentHealth { get { return currentHealth; } }

        public bool IsDead { get { return currentHealth <= 0; } }

        public event Action OnDamaged;
        public event Action OnDeath;

        void Start()
        {
            currentHealth = MaxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if(OnDamaged != null) OnDamaged();
            if ((IsDead))
            {
                Die();
            }
        }

        public void TakeDamage(int damage, Vector3 hitDirection)
        {
            if (IsDead) return;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if(hitDirection.sqrMagnitude > 0.01f)
            {
                CharacterController cc = GetComponent<CharacterController>();
                if(cc != null) cc.Move(hitDirection.normalized * 0.3f);
            }
            StartCoroutine(FlashRed());
            if(OnDamaged != null) OnDamaged();
            if(IsDead)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
            if (OnDamaged != null) OnDamaged();
        }

        public void Restore()
        {
            currentHealth = MaxHealth;
            enabled = true;
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
        }

        private void Die()
        {
            if (OnDeath != null) OnDeath();
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            enabled = false;
        }

        private IEnumerator FlashRed()
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if(renderer == null) yield break;
            Color original = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = original;
        }
    }
}
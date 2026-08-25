using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    public class AttackHit : MonoBehaviour
    {
        public int damage = 10;
        public float lifeTime = 0.1f;
        public GameObject owner;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform)) return;

            Health h = other.GetComponentInParent<Health>();
            if(h != null)
            {
                Vector3 dir = other.transform.position - owner.transform.position;
                h.TakeDamage(damage, dir);
            }
        }
    }
}
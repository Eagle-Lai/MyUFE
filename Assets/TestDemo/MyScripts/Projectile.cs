using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    public class Projectile : MonoBehaviour
    {
        public int damage = 20;
        public float speed = 15f;
        public GameObject owner;

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform)) return;

            Health h = other.GetComponentInParent<Health>();
            if(h != null)
            {
                Vector3 dir = other.transform.position - transform.position;
                h.TakeDamage(damage, dir);
                Destroy(gameObject);
            }
        }
    }
}
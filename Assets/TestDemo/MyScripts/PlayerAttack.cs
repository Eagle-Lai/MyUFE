using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts
{
    public class PlayerAttack : MonoBehaviour
    {
        public float attackCoolDown = 0.8f;
        public float lungeSpeed = 12f;
        public float lungeTime = 0.15f;
        public int attackDamage = 10;

        private float lastAttackTime = -1000;

        CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                TryAttack();
            }
        }

        private void TryAttack()
        {
            if(Time.time - lastAttackTime < attackCoolDown)
            {
                return;
            }
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            float end = Time.time + lungeTime;
            while(Time.time < end)
            {
                characterController.Move(transform.forward * lungeSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
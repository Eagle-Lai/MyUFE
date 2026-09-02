using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts
{
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState 
        { 
            None,
            Idle, 
            Chase, 
            Attack
        }

        public float detectRange = 8f;
        public float attackRange = 1.8f;
        public float moveSpeed = 3f;
        public float attackInterval = 2f;
        public int attackDamage = 2;
        public Transform player;

        private AIState aiState = AIState.Idle;
        private float lastAttackTime;
        CharacterController cc;
        Health selfHealth;

        public Animator animator;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if(animator == null)
            {
                animator = GetComponent<Animator>();
            }
            selfHealth = GetComponent<Health>();
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            if(player == null) return;

            if( selfHealth != null && selfHealth.IsDead) return;
            float dist = Vector3.Distance(transform.position, player.position);
            switch (aiState) 
            {
                case AIState.Idle:
                    if ((animator != null))
                    {
                        animator.SetFloat("Speed", 0f);
                    }
                    if (dist <= detectRange)
                    {
                        aiState = AIState.Chase;
                    }
                    break;
                case AIState.Chase:
                    FaceToPlayer();
                    cc.Move(transform.forward * moveSpeed * Time.deltaTime);
                    if((animator != null))
                    {
                        animator.SetFloat("Speed", 1f);
                    }
                    if (dist <= attackRange)
                    {
                        aiState = AIState.Attack;
                    }
                    else if(dist > detectRange * 1.5f)
                    {
                        aiState = AIState.Idle;
                    }
                    break;
                 case AIState.Attack:
                    FaceToPlayer();
                    if(animator != null)
                    {
                        animator.SetFloat("Speed", 0f);
                    }
                    if(Time.time - lastAttackTime >= attackInterval)
                    {
                        lastAttackTime = Time.time;
                        if(animator!= null)
                        {
                            animator.SetTrigger("Attack");
                        }
                        Health health = player.GetComponent<Health>();
                        if (health!= null)
                        {
                            health.TakeDamage(attackDamage, transform.position - player.position);
                        }
                        // Attack logic here, e.g., reduce player health
                        Debug.Log("Enemy attacks the player for " + attackDamage + " damage.");
                    }
                    if(dist > attackRange * 1.2f)
                    {
                        aiState = AIState.Chase;
                    }
                    break;
            }
        }

        private void FaceToPlayer()
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if ((dir.sqrMagnitude > 0.01f))
            {
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            }
        }
    }
}
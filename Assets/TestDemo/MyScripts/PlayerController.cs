using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float rotateSpeed = 10f;
        public float gravity = 9.8f;

        CharacterController cc;
        
        private void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Transform camera = Camera.main.transform;
            Vector3 camearForward = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(camera.right, Vector3.up).normalized;
            Vector3 moveDir = camearForward * v + cameraRight * h;
            if(moveDir.sqrMagnitude > 1f) moveDir = moveDir.normalized;

            if(moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            cc.Move((moveDir * moveSpeed + Vector3.down * gravity) * Time.deltaTime);
        }
    }
}
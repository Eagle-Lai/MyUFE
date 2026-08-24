using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target; // ¸úËæÄ¿±ê

        public float distance = 5f;
        public float rotateSpeed = 1f;
        public float pitchMin = -20f;
        public float pitchMax = 60f;

        float yaw;
        float pitch;

        private void Start()
        {
            if (target == null) return;

            Vector3 diff = target.position - transform.position;
            distance = Mathf.Max(diff.magnitude, 0.2f);
            yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
            float value = Mathf.Sqrt(diff.x * diff.x + diff.z * diff.z);
            pitch = Mathf.Atan2(diff.y, value) * Mathf.Rad2Deg;
        }

        private void Update()
        {
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * rotateSpeed;
                pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }
            distance -= Input.GetAxis("Mouse ScrollWheel") * 1f;
            distance = Mathf.Clamp(distance, 3f, 15f);
        }

        private void LateUpdate()
        {
            if(target == null) return;

            Quaternion quaternion = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position +quaternion * Vector3.forward * (-distance);
            transform.rotation = Quaternion.LookRotation(target.position - transform.position);
        }
    }
}
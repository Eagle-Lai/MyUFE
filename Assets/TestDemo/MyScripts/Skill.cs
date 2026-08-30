using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyScripts
{
    [System.Serializable]
    public class Skill
    {
        public string name = "ÄÜÁ¿Çò";
        public int damage = 20;
        public float coolDown = 3f;
        public float speed = 15f;
        [HideInInspector]public float lastUsedTime = -999;
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class WeaponSmoke : Weapon
    {
        public float timeValue;

        public WeaponCode code { get { return base._code; } }

        void Awake()
        {
            base.time = timeValue;
        }

        void Start()
        {
            //생성 몇 초 후 자동 파괴
            Destroy(gameObject, base.time);
        }

        public new void SetCode(WeaponCode input)
        {
            base.SetCode(input);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Vector3 destiPos = other.transform.position + (other.transform.position - transform.position) * GetComponent<SphereCollider>().radius / 2;
                destiPos.Set(destiPos.x, other.transform.position.y, destiPos.z);
                Debug.Log("Enemy: "+other.transform.position + ", Smoke: " + transform.position + ", Diff: " + destiPos);
                other.transform.GetComponent<EnemyController>().Detect(code, transform, destiPos);
            }
        }
    }
}

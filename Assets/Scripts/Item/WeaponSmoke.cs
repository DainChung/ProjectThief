using System.Collections;
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
            if (other.gameObject.layer == PhysicsLayers.Enemy)
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position, transform.GetComponent<SphereCollider>().radius, transform.forward, 0.01f, 1 << PhysicsLayers.Enemy);
                Vector3 destiPos;

                foreach (RaycastHit enemy in hits)
                {
                    destiPos = enemy.transform.position - Vector3.Normalize(transform.position - enemy.transform.position) * GetComponent<SphereCollider>().radius;
                    destiPos.Set(destiPos.x, enemy.transform.position.y, destiPos.z);
                    enemy.transform.GetComponent<EnemyController>().Detect(code, transform, destiPos);
                    try
                    {
                        enemy.transform.GetChild(0).Find("EnemyRadarEye").GetComponent<MeshCollider>().enabled = false;
                    }
                    catch (System.Exception) { }
                }
            }

            if(other.gameObject.layer == PhysicsLayers.Structure) PlayAudio();
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position, transform.GetComponent<SphereCollider>().radius, transform.forward, 0.01f, 1 << PhysicsLayers.Enemy);

                foreach (RaycastHit enemy in hits)
                {
                    EnemyController enemyController = enemy.transform.GetComponent<EnemyController>();
                    if (enemyController.curTargetCode != WeaponCode.SMOKE)
                        enemyController.InitCurTarget();
                }
            }
        }
    }
}

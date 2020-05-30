using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class AttackCollider : MonoBehaviour
    {
        private int _damage;
        private SphereCollider collider;

        public bool enableCollider { get { return collider.enabled; } set { collider.enabled = value; } }
        public int damage { get { return _damage; } }

        void Start()
        {
            collider = GetComponent<SphereCollider>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy
                || other.gameObject.layer == PhysicsLayers.Player)
            {
                Vector3 lookPos = transform.position;
                lookPos.Set(0, lookPos.y, 0);
                other.transform.GetComponent<Unit>().HitHealth(_damage, lookPos);
                collider.enabled = false;
            }
        }

        public void InitAttackCollider(int damage)
        {
            _damage = damage;
            try
            {
                collider.enabled = false;
            }
            catch (System.NullReferenceException)
            {
                collider = GetComponent<SphereCollider>();
                collider.enabled = false;
            }
        }
    }
}

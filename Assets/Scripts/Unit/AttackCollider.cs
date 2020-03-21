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

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy
                || other.gameObject.layer == PhysicsLayers.Player)
            {
                other.transform.GetComponent<Unit>().HitHealth(_damage);
                collider.enabled = false;
                Debug.Log(LayerMask.LayerToName(other.gameObject.layer) + " 피격, 데미지: " + _damage);
            }
        }

        public void InitAttackCollider(int damage)
        {
            _damage = damage;
            collider = GetComponent<SphereCollider>();
            collider.enabled = false;
        }
        public void EnableCollider(bool enable)
        {
            collider.enabled = enable;
        }
    }
}

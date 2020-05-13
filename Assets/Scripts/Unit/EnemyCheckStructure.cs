using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyCheckStructure : MonoBehaviour
    {
        private Ray ray = new Ray();
        private Vector3 h = new Vector3(0, 1f, 0);
        private bool _isThereStructure = false;
        public bool isThereStructure { get { return _isThereStructure; } }

        public void CheckStructure(Vector3 destiPos)
        {
            if (destiPos != ValueCollections.initPos)
            {
                destiPos += h;
                ray.origin = transform.position;
                ray.direction = destiPos - transform.position;

                RaycastHit[] hits = Physics.SphereCastAll(ray, 0.3f, Vector3.Distance(transform.position, destiPos), (1 << PhysicsLayers.Structure | 1 << PhysicsLayers.Weapon));
                Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(transform.position, destiPos), Color.red);

                if (hits.Length == 0) _isThereStructure = false;
                else _isThereStructure = true;
            }
        }

        public bool CheckStructureRay(Vector3 destiPos)
        {
            bool result = false;

            if (destiPos != ValueCollections.initPos)
            {
                destiPos += h;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, destiPos - transform.position, Vector3.Distance(transform.position, destiPos), (1 << PhysicsLayers.Structure | 1 << PhysicsLayers.Weapon));
                //Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(transform.position, destiPos), Color.blue);

                if (hits.Length == 0) result = false;
                else result = true;
            }

            return result;
        }
        public void InitIsThereStructure()
        {
            _isThereStructure = false;
        }
    }

}

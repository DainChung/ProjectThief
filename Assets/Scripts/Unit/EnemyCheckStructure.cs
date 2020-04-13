using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyCheckStructure : MonoBehaviour
    {
        private bool _isThereStructure = false;
        public bool isThereStructure { get { return _isThereStructure; } }

        public IEnumerator CheckStructure(Vector3 destiPos)
        {
            Ray ray = new Ray();
            ray.origin = transform.position;
            ray.direction = destiPos - transform.position;

            RaycastHit[] hits = Physics.SphereCastAll(ray, 0.5f, Vector3.Distance(transform.position, destiPos), 1 << PhysicsLayers.Structure);

            if (hits.Length > 0) _isThereStructure = true;
            else                 _isThereStructure = false;

            yield break;
        }
    }

}

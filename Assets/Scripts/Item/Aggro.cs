using System.Collections;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Aggro : MonoBehaviour
    {
        private WeaponCode code;

        public void SetCode(WeaponCode weaponCode, float radius)
        {
            GetComponent<SphereCollider>().radius = radius;
            code = weaponCode;
        }

        public IEnumerator Disposer()
        {
            yield return new WaitForSeconds(2.0f);
            gameObject.SetActive(false);

            yield break;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
                other.transform.GetComponent<EnemyController>().Detect(code, transform, transform.position);
        }
    }
}

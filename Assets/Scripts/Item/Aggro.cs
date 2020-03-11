using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Aggro : MonoBehaviour
    {
        private WeaponCode code;

        public void SetCode(WeaponCode weaponCode)
        {
            code = weaponCode;
        }

        void Start()
        {
            //소음 파일 찾아서(?) 재생

            //지속시간 짧음
            Destroy(gameObject, 2.0f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                other.transform.GetComponent<EnemyController>().DetectWeapon(code, transform.position);
        }
    }
}

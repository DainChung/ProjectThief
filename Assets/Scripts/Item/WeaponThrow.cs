using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class WeaponThrow : Weapon
    {
        private const float throwPower = 12f;
        private bool lockAggro = false;

        public float timeValue;

        public new void SetCode(WeaponCode code)
        {
            base._code = code;
        }

        void Awake()
        {
            base.rb = transform.GetComponent<Rigidbody>();
            base.time = timeValue;
        }

        // Start is called before the first frame update
        void Start()
        {
            base.rb.AddForce(transform.forward * throwPower, ForceMode.Impulse);
            //생성 몇 초 후 자동 파괴
            Destroy(gameObject, base.time);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!lockAggro)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Structure"))
                {
                    lockAggro = true;
                    GameObject obj = Instantiate(Resources.Load(FilePaths.weaponPath + "Aggro") as GameObject, transform.position, transform.rotation) as GameObject;
                    obj.GetComponent<Aggro>().SetCode(base._code);
                }
            }
        }
    }
}

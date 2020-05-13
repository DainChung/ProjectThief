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
            base.SetCode(code);
            transform.GetComponent<Item>().SetItem(code);
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

        void FixedUpdate()
        {
            if (transform.parent != null) transform.position = transform.parent.position;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Structure)
            {
                if (!lockAggro)
                {
                    lockAggro = true;
                    GameObject obj = Instantiate(Resources.Load(string.Format("{0}Aggro",FilePaths.weaponPath)) as GameObject, transform.position, transform.rotation) as GameObject;
                    obj.GetComponent<Aggro>().SetCode(base._code, 0.5f);
                }

                PlayAudio();
            }
        }
    }
}

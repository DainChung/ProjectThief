using System.Collections;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class WeaponThrow : Weapon
    {
        private const float throwPower = 12f;
        private GameObject aggroObj;
        private Aggro aggro;

        public float timeValue;
        public bool lockAggro = false;

        public WeaponCode code;

        void Awake()
        {
            base.rb = transform.GetComponent<Rigidbody>();
            base.time = timeValue;
        }

        void Start()
        {
            Init();
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
                    PoolAggro();
                }

                PlayAudio();
            }
        }


        public void Init()
        {
            base.SetCode(code);
            base.SetAudio();
            transform.GetComponent<Item>().SetItem(code);
            SetAggro();
        }
        public void SetAggro()
        {
            aggroObj = transform.Find("WeaponAggro").gameObject;

            aggro = aggroObj.GetComponent<Aggro>();
            aggroObj.SetActive(false);
            aggro.SetCode(base._code, 5.0f);
        }
        public void AddForce()
        {
            base.rb.AddForce(transform.forward * throwPower, ForceMode.Impulse);
        }
        public void PoolAggro()
        {
            try
            {
                aggroObj.SetActive(true);
                StartCoroutine(aggro.Disposer());
            }
            catch (System.NullReferenceException)
            {
                SetAggro();
                aggroObj.SetActive(true);
                StartCoroutine(aggro.Disposer());
            }
        }
        public IEnumerator Disposer()
        {
            yield return new WaitForSeconds(timeValue);
            lockAggro = false;
            gameObject.SetActive(false);

            yield break;
        }

        public void DisposeImmediately()
        {
            lockAggro = false;
            gameObject.SetActive(false);
        }
    }
}

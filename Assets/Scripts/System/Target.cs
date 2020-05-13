using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Target : MonoBehaviour
    {
        private WeaponCode _code;
        private int _id;
        private float time = 60.0f;
        private EnemyController _enemy;
        public WeaponCode code { get { return _code; } }
        public int ID { get { return _id; } }

        void FixedUpdate()
        {
            if (_enemy.curTargetPos != transform.position) Destroy(gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Out"))
            {
                _enemy.InitCurTarget();
                Destroy(gameObject);
            }
        }

        void DelayDestroy()
        {
            //생성후 600초 경과시 자동 삭제
            Destroy(gameObject, time);
        }
        public void SetTarget(int id, WeaponCode code, EnemyController enemy)
        {
            _id = id;
            _code = code;
            _enemy = enemy;

            if (_code == WeaponCode.PLAYER) time = 15.0f;
            else time = 60.0f;

            if (transform.position == ValueCollections.initPos) Destroy(this);
            DelayDestroy();
        }
    }
}

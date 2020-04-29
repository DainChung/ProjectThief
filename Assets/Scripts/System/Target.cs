using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Target : MonoBehaviour
    {
        private WeaponCode _code;
        private int _id;
        public WeaponCode code { get { return _code; } }
        public int ID { get { return _id; } }

        void Start()
        {
            //생성후 600초 경과시 자동 삭제
            Destroy(gameObject, 600.0f);
        }

        public void SetTarget(int id, WeaponCode code)
        {
            _id = id;
            _code = code;
        }
    }
}

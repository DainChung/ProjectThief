using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Target : MonoBehaviour
    {
        private int _id;
        public int ID { get { return _id; } }

        //Enemy가 무시하는 경우 Target이 계속 남아있는 현상 방지
        void Start()
        {
            Destroy(gameObject, 600.0f);
        }

        public void SetID(int id)
        {
            _id = id;
        }
    }
}

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

        public void SetID(int id)
        {
            _id = id;
        }
    }
}

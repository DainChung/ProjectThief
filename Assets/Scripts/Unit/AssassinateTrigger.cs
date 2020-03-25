using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class AssassinateTrigger : MonoBehaviour
    {
        private bool _canAssassinate = false;
        public bool canAssassinate { get { return _canAssassinate; } }

        void OnTriggerEnter(Collider other)
        {

        }
    }
}
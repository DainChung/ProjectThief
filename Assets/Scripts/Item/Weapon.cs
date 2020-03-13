using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Weapon : MonoBehaviour
    {
        protected Rigidbody rb;
        protected float time;
        protected WeaponCode _code;

        protected void SetCode(WeaponCode input)
        {
            _code = input;
        }
    }
}

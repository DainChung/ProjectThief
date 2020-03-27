using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class AnimatorEventTOUnit : MonoBehaviour
    {
        public Unit unit;

        public void DisableAssassinate()
        {
            unit.EnableAssassinate(false);
            unit.lockControl = false;
        }

        public void Dead()
        {
            unit.animator.speed = 0;
            unit.Dead();
        }
    }
}

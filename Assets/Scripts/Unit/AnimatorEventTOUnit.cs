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

            unit.swManager.RestartAttackStopwatch((int)WeaponCode.HAND);
        }

        public void DisableAttackDefault()
        {
            unit.animator.speed = 1;
            unit.EnableDefaultAttack(false);
            unit.animator.SetBool("IsAttack", false);
            unit.swManager.RestartAttackStopwatch((int)WeaponCode.HAND);
            unit.swManager.attackCountDelay.Restart();
            unit.curUnitPose = UnitPose.MOD_RUN;

            unit.transform.GetComponent<Unit>().curLookDir = LookDirState.IDLE;
        }

        public void Dead()
        {
            unit.animator.speed = 0;
            unit.Dead();
        }

        public void UnlockControl()
        {
            unit.lockControl = false;
        }
        public void LockControl()
        {
            unit.lockControl = true;
        }
    }
}

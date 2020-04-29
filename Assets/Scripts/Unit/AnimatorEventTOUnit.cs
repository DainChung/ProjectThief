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

            unit.swManager.RestartSW((int)WeaponCode.HAND);
        }
        public void DisableAttackDefault()
        {
            unit.EnableDefaultAttack(false);
            unit.animator.SetBool("IsAttack", false);
            unit.animator.SetBool("IsHit", false);
            unit.swManager.RestartSW((int)WeaponCode.HAND);
            unit.swManager.attackCountDelay.Restart();
            unit.curUnitPose = UnitPose.MOD_RUN;

            unit.transform.GetComponent<Unit>().curLookDir = LookDirState.IDLE;
        }

        public void Dead()
        {
            unit.animator.speed = 0;
            unit.Dead();
        }

        public void Assassinated()
        {
            unit.HitHealth(-1, Vector3.zero);
        }

        public void UnlockControl()
        {
            unit.animator.SetBool("IsHit", false);
            unit.lockControl = false;
        }
        public void LockControl()
        {
            unit.lockControl = true;
        }
    }
}

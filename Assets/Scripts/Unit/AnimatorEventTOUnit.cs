using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.GameSystem;

namespace Com.MyCompany.MyGame
{
    public class AnimatorEventTOUnit : MonoBehaviour
    {
        private Unit unit;
        private AudioManager audioManager;

        void Start()
        {
            unit = transform.parent.GetComponent<Unit>();
            audioManager = transform.parent.GetComponent<AudioManager>();
        }

        public void DisableAssassinate()
        {
            unit.EnableAssassinate(false);
            unit.lockControl = false;

            unit.swManager.RestartSW((int)WeaponCode.HAND);
        }
        public void DisableAttackDefault()
        {
            unit.EnableDefaultAttack(false);
            unit.swManager.RestartSW((int)WeaponCode.HAND);
            unit.swManager.attackCountDelay.Restart();
            unit.curUnitPose = UnitPose.MOD_RUN;

            unit.transform.GetComponent<Unit>().curLookDir = LookDirState.IDLE;

            unit.animator.SetBool("IsAttack", false);
            unit.animator.SetBool("IsHit", false);
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

        public void PlayAudio(string name)
        {
            audioManager.PlayAudio(name);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyAnimationController : MonoBehaviour
    {
        #region Private Fields

        private EnemyController enemy;
        private Animator animator;
        private Unit unit;
        private UnitAnimationController unitAnimController;
        private CapsuleCollider collider;

        const float crouchColliderHeight = 1.5f;
        const float standColliderHeight = 90f;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            unit = GetComponent<Unit>();
            enemy = GetComponent<EnemyController>();
            animator = unit.animator;

            collider = GetComponent<CapsuleCollider>();

            unitAnimController = unit.unitAnimController;
        }

        void FixedUpdate()
        {
            ControlEnemyAnimation();
        }

        #endregion

        #region Private Methods

        void ControlEnemyAnimation()
        {
            if (animator.GetFloat("MoveSpeed") != 0 || animator.GetInteger("DeadAnim") != 0)
            {
                if (animator.GetCurrentAnimatorStateInfo(AnimationLayers.Standing).IsTag("Idle"))
                    animator.Play("Running", AnimationLayers.Standing);
            }

            switch (unit.curUnitPose)
            {
                #region MOD_WALK(== UnitState.IDLE)
                case UnitPose.MOD_WALK:
                    if (unit.curUnitState == UnitState.ALERT)
                    {
                        unitAnimController.WalkPoseTONewPose(UnitPose.MOD_CROUCH);
                        enemy.EnemyAlertManager();
                    }
                    else if (unit.curUnitState == UnitState.COMBAT)
                    {
                        unitAnimController.WalkPoseTONewPose(UnitPose.MOD_ATTACK);
                        enemy.EnemyAlertManager();
                    }
                    else if (unit.curUnitState == UnitState.CHEESE)
                    {
                        unitAnimController.WalkPoseTONewPose(UnitPose.MOD_RUN);
                        enemy.EnemyAlertManager();
                    }

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight);
                    break;
                #endregion

                #region MOD_RUN(== UnitState.CHEESE || UnitState.COMBAT)
                case UnitPose.MOD_RUN:
                    if (unit.curUnitState == UnitState.IDLE)
                    {
                        unitAnimController.RunPoseTONewPose(UnitPose.MOD_WALK);
                        enemy.EnemyAlertManager();
                    }
                    else if (unit.curUnitState == UnitState.ALERT)
                    {
                        unitAnimController.RunPoseTONewPose(UnitPose.MOD_CROUCH);
                        enemy.EnemyAlertManager();
                    }

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight);
                    break;
                #endregion

                #region MOD_CROUCH(== UnitState.ALERT)
                case UnitPose.MOD_CROUCH:
                    if (unit.curUnitState == UnitState.IDLE)
                    {
                        unitAnimController.CrouchPoseTONewPose(UnitPose.MOD_WALK);
                        enemy.EnemyAlertManager();
                    }
                    else if (unit.curUnitState == UnitState.COMBAT || unit.curUnitState == UnitState.CHEESE)
                    {
                        unitAnimController.CrouchPoseTONewPose(UnitPose.MOD_RUN);
                        enemy.EnemyAlertManager();
                    }

                    unitAnimController.SmoothStanding(collider, crouchColliderHeight);
                    break;
                #endregion

                #region MOD_INSMOKE(== UnitState.INSMOKE)
                case UnitPose.MOD_INSMOKE:
                    animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                    break;
                #endregion

                #region MOD_ATTACK( == UnitState.COMBAT)
                case UnitPose.MOD_ATTACK:
                    if (!animator.GetBool("IsRunMode"))
                        animator.SetBool("IsRunMode", true);
                    break;
                #endregion

                default:
                    break;
            }

            if (unit.IsOnFloor())
            {
                switch (unit.curUnitPose)
                {
                    case UnitPose.MOD_WALK:
                        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                            animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                        break;
                    case UnitPose.MOD_RUN:
                    case UnitPose.MOD_CROUCH:
                        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                            animator.SetFloat("MoveSpeed", enemy.moveSpeed);

                        break;
                    case UnitPose.MOD_ATTACK:
                        animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}

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

                #region MOD_RUN(== UnitState.CHEESE)
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

                    //개발중
                #region MOD_COVERSTAND(미정)
                case UnitPose.MOD_COVERSTAND:
                    break;
                #endregion

                    //개발중
                #region MOD_COVERCROUCH(미정)
                case UnitPose.MOD_COVERCROUCH:
                    break;
                #endregion

                    //개발중
                #region MOD_THROW(== UnitState.COMBAT)
                case UnitPose.MOD_THROW:
                    if (unit.IsOnFloor())
                    {
                        ////조준하는 동안 이동 애니메이션 제어
                        //animator.SetFloat("MoveSpeed", enemyVerticalMove);
                        //animator.SetFloat("TurnRight", enemyHorizontalMove);

                        //Animation Layer 제어
                        unitAnimController.ControlThrowLayer();
                    }
                    break;
                #endregion

                    //개발중
                    //Mixamo의 Drunk 시리즈를 사용할 것
                #region MOD_INSMOKE(== UnitState.INSMOKE)
                case UnitPose.MOD_INSMOKE:
                    animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                    break;
                #endregion

                    //개발중
                    //Mixamo에서 Sleep 관련 애니메이션을 찾을 것
                #region MOD_SLEEP( == UnitState.SLEEP)
                case UnitPose.MOD_SLEEP:
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
                    //개발 중
                    #region Control Basic Move Animation
                    case UnitPose.MOD_WALK:
                        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                        {
                            animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                            //Debug.Log("animator:" + animator.GetFloat("MoveSpeed") + ", enemyController: " + enemy.moveSpeed);
                        }
                        break;
                    case UnitPose.MOD_RUN:
                    case UnitPose.MOD_CROUCH:

                        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                        {
                            animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                            //        if (Input.GetButton("Vertical"))
                            //        {
                            //            animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Vertical")));
                            //        }
                            //        else if (Input.GetButton("Horizontal"))
                            //        {
                            //            animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));
                            //        }
                            //        else
                            //        {
                            //            animator.SetFloat("TurnRight", 0);
                            //            animator.SetFloat("MoveSpeed", 0);
                            //        }
                        }

                        break;
                    #endregion

                    //개발 중(미정)
                    #region Control Cover Move Animation
                    //case UnitPose.MOD_COVERSTAND:
                    //case UnitPose.MOD_COVERCROUCH:

                    //    if (unit.IsWallClose())
                    //    {
                    //        animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));

                    //        // 양수면 오른쪽, 음수면 왼쪽
                    //        animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));

                    //        if (animator.GetFloat("TurnRight") > 0)
                    //            animator.SetBool("LookRight", true);
                    //        else if (animator.GetFloat("TurnRight") < 0)
                    //            animator.SetBool("LookRight", false);
                    //    }

                    //    break;
                    #endregion

                    #region MOD_ATTACK
                    case UnitPose.MOD_ATTACK:
                        animator.SetFloat("MoveSpeed", enemy.moveSpeed);
                        break;
                    #endregion

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}

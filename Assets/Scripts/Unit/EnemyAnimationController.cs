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

        const float crouchColliderHeight = 64f;
        const float standColliderHeight = 90f;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            unit = GetComponent<Unit>();
            enemy = GetComponent<EnemyController>();
            animator = unit.animator;

            unitAnimController = new UnitAnimationController(unit, animator);
            collider = GetComponent<CapsuleCollider>();
        }

        void FixedUpdate()
        {

        }

        #endregion

        #region Private Methods

        void ControlEnemyAnimation()
        {
            switch (unit.curUnitPose)
            {
                #region MOD_WALK(== curUnitState.IDLE)
                case UnitPose.MOD_WALK:
                    break;
                #endregion

                #region MOD_RUN(== curUnitState.COMBAT)
                case UnitPose.MOD_RUN:
                    break;
                #endregion

                #region MOD_CROUCH(== curUnitState.ALERT)
                case UnitPose.MOD_CROUCH:
                    break;
                #endregion

                #region MOD_COVERSTAND(미정)
                case UnitPose.MOD_COVERSTAND:
                    break;
                #endregion

                #region MOD_COVERCROUCH(미정)
                case UnitPose.MOD_COVERCROUCH:
                    break;
                #endregion

                #region MOD_THROW(== curUnitState.COMBAT)
                case UnitPose.MOD_THROW:
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
                    //case UnitPose.MOD_WALK:
                    //case UnitPose.MOD_RUN:
                    //case UnitPose.MOD_CROUCH:

                    //    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                    //    {
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
                    //    }

                    //    break;
                    #endregion

                    //개발 중
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

                    default:
                        break;
                }
            }
            else
            {
                animator.SetBool("IsCovering", false);
                animator.SetFloat("TurnRight", 0);
                animator.SetFloat("MoveSpeed", 0);
            }
        }

        #endregion

        #region Public Methods
        #endregion
    }
}

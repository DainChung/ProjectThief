using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{

    public class PlayerAnimationController : MonoBehaviour
    {
        #region Private Fields

        private PlayerController player;
        private Animator animator;
        private Unit unit;
        private UnitAnimationController unitAnimController;
        private CapsuleCollider collider;

        const float crouchColliderHeight = 64f;
        const float standColliderHeight = 90f;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            unit = GetComponent<Unit>();
            player = GetComponent<PlayerController>();
            animator = unit.animator;

            unitAnimController = unit.unitAnimController;
            collider = GetComponent<CapsuleCollider>();
        }

        void FixedUpdate()
        {
            ControlPlayerAnimation();
        }

        #endregion

        #region Private Methods

        void ControlPlayerAnimation()
        {
            switch (unit.curUnitPose)
            {
                #region MOD_WALK
                case UnitPose.MOD_WALK:
                    //MOD_WALK -> MOD_RUN
                    if (Input.GetButtonDown("Walk"))
                    {
                        unitAnimController.WalkPoseTONewPose(UnitPose.MOD_RUN);
                        player.SetBYCurUnitPose();
                        break;
                    }

                    //MOD_WALK -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        unitAnimController.WalkPoseTONewPose(UnitPose.MOD_COVERSTAND);
                        player.SetBYCurUnitPose();
                        break;
                    }

                    //MOD_WALK -> MOD_CROUCH
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.WalkPoseTONewPose(UnitPose.MOD_CROUCH);

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight, player);

                    break;
                #endregion

                #region MOD_RUN
                case UnitPose.MOD_RUN:
                    //MOD_RUN -> MOD_WALK
                    if (Input.GetButtonDown("Walk"))
                    {
                        unitAnimController.RunPoseTONewPose(UnitPose.MOD_WALK);
                        player.SetBYCurUnitPose();
                        break;
                    }

                    //MOD_RUN -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        unitAnimController.RunPoseTONewPose(UnitPose.MOD_COVERSTAND);
                        player.SetBYCurUnitPose();
                        break;
                    }

                    //MOD_RUN -> MOD_CROUCH
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.RunPoseTONewPose(UnitPose.MOD_CROUCH);

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight, player);
                    break;
                #endregion

                #region MOD_CROUCH
                case UnitPose.MOD_CROUCH:
                    //MOD_CROUCH -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        unitAnimController.CrouchPoseTONewPose(UnitPose.MOD_COVERCROUCH);
                        player.SetBYCurUnitPose();
                        break;
                    }

                    //MOD_CROUCH -> MOD_RUN
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.CrouchPoseTONewPose(UnitPose.MOD_RUN);

                    unitAnimController.SmoothStanding(collider, standColliderHeight, player);
                    break;
                #endregion

                #region MOD_COVERSTAND
                case UnitPose.MOD_COVERSTAND:
                    //MOD_COVERSTAND -> MOD_RUN
                    if (Input.GetButtonDown("Covering"))
                    {
                        unitAnimController.CoverStandingPoseTONewPose(UnitPose.MOD_RUN);
                        player.SetBYCurUnitPose();

                        break;
                    }

                    //MOD_COVERSTAND -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.CoverStandingPoseTONewPose(UnitPose.MOD_COVERCROUCH);

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight, player);
                    break;
                #endregion

                #region MOD_COVERCROUCH
                case UnitPose.MOD_COVERCROUCH:
                    //MOD_COVERCROUCH -> MOD_CROUCH
                    if (Input.GetButtonDown("Covering"))
                    {
                        unitAnimController.CoverCrouchPoseTONewPose(UnitPose.MOD_CROUCH);
                        player.SetBYCurUnitPose();
                        break;
                    }

                    //MOD_COVERCROUCH -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.CoverCrouchPoseTONewPose(UnitPose.MOD_COVERSTAND);

                    unitAnimController.SmoothStanding(collider, standColliderHeight, player);
                    break;
                #endregion

                #region MOD_THROW
                case UnitPose.MOD_THROW:
                    //Control Throw Move Animation
                    //조준하는 동안 애니메이션 제어
                    if (unit.IsOnFloor())
                    {
                        //조준하는 동안 이동 애니메이션 제어
                        if (Input.GetButton("Vertical"))
                            animator.SetFloat("MoveSpeed", Input.GetAxis("Vertical"));
                        else
                            animator.SetFloat("MoveSpeed", 0);

                        if (Input.GetButton("Horizontal"))
                            animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));
                        else
                            animator.SetFloat("TurnRight", 0);

                        //Animation Layer 제어
                        unitAnimController.ControlThrowLayer();
                    }
                    break;
                #endregion

                default:
                    break;
            }

            if (unit.IsOnFloor())
            {
                switch (unit.curUnitPose)
                {
                    #region Control Basic Move Animation
                    case UnitPose.MOD_WALK:
                    case UnitPose.MOD_RUN:
                    case UnitPose.MOD_CROUCH:

                        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                        {
                            if (Input.GetButton("Vertical"))
                            {
                                animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Vertical")));
                            }
                            else if (Input.GetButton("Horizontal"))
                            {
                                animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));
                            }
                            else
                            {
                                animator.SetFloat("TurnRight", 0);
                                animator.SetFloat("MoveSpeed", 0);
                            }
                        }

                        break;
                    #endregion

                    #region Control Cover Move Animation
                    case UnitPose.MOD_COVERSTAND:
                    case UnitPose.MOD_COVERCROUCH:

                        if (unit.IsWallClose())
                        {
                            animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));

                            // 양수면 오른쪽, 음수면 왼쪽
                            animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));

                            if (animator.GetFloat("TurnRight") > 0)
                                animator.SetBool("LookRight", true);
                            else if (animator.GetFloat("TurnRight") < 0)
                                animator.SetBool("LookRight", false);
                        }

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

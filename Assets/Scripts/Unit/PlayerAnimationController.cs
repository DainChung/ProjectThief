using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{

    public class PlayerAnimationController : MonoBehaviour
    {
        #region Private Fields

        private float animCrouchLayerWeight = 0f;

        private PlayerController player;
        private Animator animator;
        private Unit unit;

        private UnitAnimationController unitAnimController;

        private CapsuleCollider collider;

        const float crouchColliderHeight = 64f;
        const float standColliderHeight = 90f;

        #endregion

        #region Public Fields

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            unit = GetComponent<Unit>();
            player = GetComponent<PlayerController>();
            animator = unit.animator;

            unitAnimController = new UnitAnimationController(unit, animator);
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
                        unitAnimController.WalkPoseToNewPose(UnitPose.MOD_RUN);
                        player.SetAggro();
                        break;
                    }

                    //MOD_WALK -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        unitAnimController.WalkPoseToNewPose(UnitPose.MOD_COVERSTAND);
                        player.SetAggro();
                        break;
                    }

                    //MOD_WALK -> MOD_CROUCH
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.WalkPoseToNewPose(UnitPose.MOD_CROUCH);

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight, player);

                    break;
                #endregion

                #region MOD_RUN
                case UnitPose.MOD_RUN:
                    //MOD_RUN -> MOD_WALK
                    if (Input.GetButtonDown("Walk"))
                    {
                        unitAnimController.RunPoseToNewPose(UnitPose.MOD_WALK);
                        player.SetAggro();
                        break;
                    }

                    //MOD_RUN -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        unitAnimController.RunPoseToNewPose(UnitPose.MOD_COVERSTAND);
                        player.SetAggro();

                        break;
                    }

                    //MOD_RUN -> MOD_CROUCH
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.RunPoseToNewPose(UnitPose.MOD_CROUCH);

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight, player);
                    break;
                #endregion

                #region MOD_CROUCH
                case UnitPose.MOD_CROUCH:
                    //MOD_CROUCH -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        unitAnimController.CrouchPoseToNewPose(UnitPose.MOD_COVERCROUCH);
                        player.SetAggro();

                        break;
                    }

                    //MOD_CROUCH -> MOD_RUN
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.CrouchPoseToNewPose(UnitPose.MOD_RUN);

                    unitAnimController.SmoothStanding(collider, standColliderHeight, player);
                    break;
                #endregion

                #region MOD_COVERSTAND
                case UnitPose.MOD_COVERSTAND:
                    //MOD_COVERSTAND -> MOD_RUN
                    if (Input.GetButtonDown("Covering"))
                    {
                        unitAnimController.CoverStandingPoseToNewPose(UnitPose.MOD_RUN);
                        player.SetAggro();

                        break;
                    }

                    //MOD_COVERSTAND -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Crouch"))
                        unitAnimController.CoverStandingPoseToNewPose(UnitPose.MOD_COVERCROUCH);

                    unitAnimController.SmoothCrouching(collider, crouchColliderHeight, player);
                    break;
                #endregion

                #region MOD_COVERCROUCH
                case UnitPose.MOD_COVERCROUCH:
                    //MOD_COVERCROUCH -> MOD_CROUCH
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", false);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(false));
                        animator.SetLayerWeight(1, 1);
                        animator.SetLayerWeight(2, 0);
                        animator.SetLayerWeight(3, 0);

                        unit.curUnitPose = UnitPose.MOD_CROUCH;
                        player.SetAggro();

                        break;
                    }

                    //MOD_COVERCROUCH -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Crouch"))
                    {
                        if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                        {
                            animator.SetLayerWeight(1, animCrouchLayerWeight);
                            animator.SetBool("IsCrouchMode", false);
                        }
                    }

                    if (animator.GetLayerWeight(1) > 0 && !animator.GetBool("IsCrouchMode"))
                    {
                        transform.GetComponent<CapsuleCollider>().height = standColliderHeight;
                        transform.GetComponent<CapsuleCollider>().center = new Vector3(0, standColliderHeight / 2, transform.GetComponent<CapsuleCollider>().center.z);

                        animCrouchLayerWeight -= (Time.deltaTime * 3f);
                        animCrouchLayerWeight = Mathf.Clamp01(animCrouchLayerWeight);
                        if (animCrouchLayerWeight == 0)
                        {
                            unit.curUnitPose = UnitPose.MOD_COVERSTAND;
                            player.SetAggro();
                        }

                        animator.SetLayerWeight(1, animCrouchLayerWeight);
                        animator.SetLayerWeight(2, 1 - animCrouchLayerWeight);
                        animator.SetLayerWeight(3, animCrouchLayerWeight);

                        break;
                    }
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
                        if (animator.GetFloat("TurnRight") == 0 && animator.GetFloat("MoveSpeed") == 0)
                        {
                            animator.SetLayerWeight(4, 1);
                            animator.SetLayerWeight(5, 0);
                        }
                        else
                        {
                            animator.SetLayerWeight(4, 0);
                            animator.SetLayerWeight(5, 1);
                        }
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

                //#region Control Move Animation

                ////WALK, RUN, CROUCH
                //if (unit.curUnitPose == UnitPose.MOD_WALK || unit.curUnitPose == UnitPose.MOD_RUN || unit.curUnitPose == UnitPose.MOD_CROUCH)
                //{
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
                //}
                //#endregion

                //#region Control Cover Move Animation

                //if (unit.IsWallClose())
                //{
                //    //COVERCROUCH, COVERSTAND
                //    if (unit.curUnitPose == UnitPose.MOD_COVERCROUCH || unit.curUnitPose == UnitPose.MOD_COVERSTAND)
                //    {
                //        animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));

                //        // 양수면 오른쪽, 음수면 왼쪽
                //        animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));

                //        if (animator.GetFloat("TurnRight") > 0)
                //            animator.SetBool("LookRight", true);
                //        else if (animator.GetFloat("TurnRight") < 0)
                //            animator.SetBool("LookRight", false);
                //    }
                //}

                //#endregion
            }
            else
            {
                animator.SetBool("IsCovering", false);
                animator.SetFloat("TurnRight", 0);
                animator.SetFloat("MoveSpeed", 0);
            }
        }

        #endregion
    }
}

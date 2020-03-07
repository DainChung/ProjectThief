using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{

    public class PlayerAnimationController : MonoBehaviour
    {
        #region Private Fields

        private float animLayerWeight = 0f;

        private Unit unit;

        const float crouchColliderHeight = 64f;
        const float standColliderHeight = 90f;

        #endregion

        #region Public Fields

        [HideInInspector]
        public Animator animator;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            unit = GetComponent<Unit>();
          
            animator = unit.animator;

            animator.SetBool("IsRunMode", true);
            animator.SetLayerWeight(1, 0);
            animator.SetBool("IsCrouchMode", false);
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
                        animator.SetBool("IsRunMode", true);
                        unit.curUnitPose = UnitPose.MOD_RUN;

                        break;
                    }

                    //MOD_WALK -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        animator.SetBool("IsCovering", true);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(unit.WallTransform().position, unit.WallTransform().right, true));
                        animator.SetLayerWeight(2, 1);
                        animator.SetLayerWeight(3, 0);
                        unit.curUnitPose = UnitPose.MOD_COVERSTAND;

                        break;
                    }

                    //MOD_WALK -> MOD_CROUCH
                    if (Input.GetButtonDown("Crouch"))
                    {
                        if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                        {
                            animator.SetLayerWeight(1, animLayerWeight);
                            animator.SetBool("IsCrouchMode", true);
                        }
                    }

                    if (animator.GetLayerWeight(1) < 1 && animator.GetBool("IsCrouchMode"))
                    {
                        animator.SetBool("IsRunMode", false);

                        transform.GetComponent<CapsuleCollider>().height = crouchColliderHeight;
                        transform.GetComponent<CapsuleCollider>().center = new Vector3(0, crouchColliderHeight / 2, transform.GetComponent<CapsuleCollider>().center.z);

                        animLayerWeight += (Time.deltaTime * 3f);
                        animLayerWeight = Mathf.Clamp01(animLayerWeight);
                        if (animLayerWeight == 1)
                            unit.curUnitPose = UnitPose.MOD_CROUCH;

                        animator.SetLayerWeight(1, animLayerWeight);
                    }

                    break;
                #endregion

                #region MOD_RUN
                case UnitPose.MOD_RUN:
                    //MOD_WALK -> MOD_RUN
                    if (Input.GetButtonDown("Walk"))
                    {
                        animator.SetBool("IsRunMode", false);
                        unit.curUnitPose = UnitPose.MOD_WALK;

                        break;
                    }

                    //MOD_RUN -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        animator.SetBool("IsCovering", true);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(unit.WallTransform().position, unit.WallTransform().right, true));
                        animator.SetLayerWeight(2, 1);
                        animator.SetLayerWeight(3, 0);
                        unit.curUnitPose = UnitPose.MOD_COVERSTAND;

                        break;
                    }

                    //MOD_WALK -> MOD_CROUCH
                    if (Input.GetButtonDown("Crouch"))
                    {
                        if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                        {
                            animator.SetLayerWeight(1, animLayerWeight);
                            animator.SetBool("IsCrouchMode", true);
                        }
                    }

                    if (animator.GetLayerWeight(1) < 1 && animator.GetBool("IsCrouchMode"))
                    {
                        animator.SetBool("IsRunMode", false);

                        transform.GetComponent<CapsuleCollider>().height = crouchColliderHeight;
                        transform.GetComponent<CapsuleCollider>().center = new Vector3(0, crouchColliderHeight / 2, transform.GetComponent<CapsuleCollider>().center.z);

                        animLayerWeight += (Time.deltaTime * 3f);
                        animLayerWeight = Mathf.Clamp01(animLayerWeight);
                        if (animLayerWeight == 1)
                            unit.curUnitPose = UnitPose.MOD_CROUCH;

                        animator.SetLayerWeight(1, animLayerWeight);

                        break;
                    }
                    //Debug.Log(unit.curUnitPose);
                    break;
                #endregion

                #region MOD_CROUCH
                case UnitPose.MOD_CROUCH:
                    //MOD_CROUCH -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Covering") && unit.IsWallClose())
                    {
                        animator.SetBool("IsCovering", true);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(unit.WallTransform().position, unit.WallTransform().right, true));
                        animator.SetLayerWeight(2, 0);
                        animator.SetLayerWeight(3, 1);
                        unit.curUnitPose = UnitPose.MOD_COVERCROUCH;

                        break;
                    }

                    //MOD_CROUCH -> MOD_RUN
                    if (Input.GetButtonDown("Crouch"))
                    {
                        if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                        {
                            animator.SetLayerWeight(1, animLayerWeight);
                            animator.SetBool("IsCrouchMode", false);
                        }
                    }

                    if (animator.GetLayerWeight(1) > 0 && !animator.GetBool("IsCrouchMode"))
                    {
                        animator.SetBool("IsRunMode", true);

                        transform.GetComponent<CapsuleCollider>().height = standColliderHeight;
                        transform.GetComponent<CapsuleCollider>().center = new Vector3(0, standColliderHeight / 2, transform.GetComponent<CapsuleCollider>().center.z);

                        animLayerWeight -= (Time.deltaTime * 3f);
                        animLayerWeight = Mathf.Clamp01(animLayerWeight);
                        if (animLayerWeight == 0)
                            unit.curUnitPose = UnitPose.MOD_RUN;

                        animator.SetLayerWeight(1, animLayerWeight);

                        break;
                    }
                    break;
                #endregion

                #region MOD_COVERSTAND
                case UnitPose.MOD_COVERSTAND:
                    //MOD_COVERSTAND -> MOD_RUN
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", false);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(unit.WallTransform().position, unit.WallTransform().right, false));
                        animator.SetLayerWeight(2, 0);
                        unit.curUnitPose = UnitPose.MOD_RUN;

                        break;
                    }

                    //MOD_COVERSTAND -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Crouch"))
                    {
                        if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                        {
                            animator.SetLayerWeight(1, animLayerWeight);
                            animator.SetBool("IsCrouchMode", true);
                        }
                    }

                    if (animator.GetLayerWeight(1) < 1 && animator.GetBool("IsCrouchMode"))
                    {
                        transform.GetComponent<CapsuleCollider>().height = crouchColliderHeight;
                        transform.GetComponent<CapsuleCollider>().center = new Vector3(0, crouchColliderHeight / 2, transform.GetComponent<CapsuleCollider>().center.z);

                        animLayerWeight += (Time.deltaTime * 3f);
                        animLayerWeight = Mathf.Clamp01(animLayerWeight);
                        if (animLayerWeight == 1)
                            unit.curUnitPose = UnitPose.MOD_COVERCROUCH;

                        animator.SetLayerWeight(1, animLayerWeight);
                        animator.SetLayerWeight(2, 1 - animLayerWeight);
                        animator.SetLayerWeight(3, animLayerWeight);

                        break;
                    }
                    break;
                #endregion

                #region MOD_COVERCROUCH
                case UnitPose.MOD_COVERCROUCH:
                    //MOD_COVERCROUCH -> MOD_CROUCH
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", false);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(unit.WallTransform().position, unit.WallTransform().right, false));
                        animator.SetLayerWeight(1, 1);
                        animator.SetLayerWeight(2, 0);
                        animator.SetLayerWeight(3, 0);
                        unit.curUnitPose = UnitPose.MOD_CROUCH;

                        break;
                    }

                    //MOD_COVERCROUCH -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Crouch"))
                    {
                        if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                        {
                            animator.SetLayerWeight(1, animLayerWeight);
                            animator.SetBool("IsCrouchMode", false);
                        }
                    }

                    if (animator.GetLayerWeight(1) > 0 && !animator.GetBool("IsCrouchMode"))
                    {
                        transform.GetComponent<CapsuleCollider>().height = standColliderHeight;
                        transform.GetComponent<CapsuleCollider>().center = new Vector3(0, standColliderHeight / 2, transform.GetComponent<CapsuleCollider>().center.z);

                        animLayerWeight -= (Time.deltaTime * 3f);
                        animLayerWeight = Mathf.Clamp01(animLayerWeight);
                        if (animLayerWeight == 0)
                            unit.curUnitPose = UnitPose.MOD_COVERSTAND;

                        animator.SetLayerWeight(1, animLayerWeight);
                        animator.SetLayerWeight(2, 1 - animLayerWeight);
                        animator.SetLayerWeight(3, animLayerWeight);

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
                #region Control Move Animation

                //WALK, RUN, CROUCH
                if (unit.curUnitPose == UnitPose.MOD_WALK || unit.curUnitPose == UnitPose.MOD_RUN || unit.curUnitPose == UnitPose.MOD_CROUCH)
                {
                    //if (animator.GetBool("IsFalling"))
                    //    animator.SetBool("IsFalling", false);

                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing") && unit.curUnitPose != UnitPose.MOD_FALL)
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
                }
                #endregion

                #region Control Cover Move Animation

                if (unit.IsWallClose())
                {
                    //COVERCROUCH, COVERSTAND
                    if (unit.curUnitPose == UnitPose.MOD_COVERCROUCH || unit.curUnitPose == UnitPose.MOD_COVERSTAND)
                    {
                        animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));

                        // 양수면 오른쪽, 음수면 왼쪽
                        animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));

                        if (animator.GetFloat("TurnRight") > 0)
                            animator.SetBool("LookRight", true);
                        else if (animator.GetFloat("TurnRight") < 0)
                            animator.SetBool("LookRight", false);
                    }
                }

                #endregion
            }
            else
            {
                //animator.SetBool("IsFalling", true);
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

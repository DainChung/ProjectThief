using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{

    public class PlayerAnimationController : MonoBehaviour
    {
        #region Private Values

        private bool _isOnFloor = false;
        private bool _isWallClose = false;

        private float animLayerWeight = 0f;

        private Unit unit;

        const float crouchColliderHeight = 64f;
        const float standColliderHeight = 90f;

        #endregion

        #region Public Values

        public bool isOnFloor { get { return _isOnFloor; } }
        public bool isWallClose { get { return _isWallClose; } }

        [HideInInspector]
        public Transform wallTransform;
        [HideInInspector]
        public Vector3 nearWallEndPos;
        [HideInInspector]
        public Vector3 wallEndToEndPos;

        public Animator animator;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            animator.SetBool("IsRunMode", true);
            animator.SetLayerWeight(1, 0);
            animator.SetBool("IsCrouchMode", false);

            unit = GetComponent<Unit>();
            nearWallEndPos = Vector3.zero;
        }

        void FixedUpdate()
        {
            ControlPlayerAnimation();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Wall"))
            {
                _isWallClose = true;
                wallTransform = other.transform;
            }

            if (other.CompareTag("WallRightEnd") && _isWallClose)
            {
                animator.SetBool("IsWallRightEnd", true);
                nearWallEndPos = other.GetComponent<WallEnd>().nearWallEndPos;
                wallEndToEndPos = other.GetComponent<WallEnd>().wallEndToEndPos;
            }

            if (other.CompareTag("WallLeftEnd") && _isWallClose)
            {
                animator.SetBool("IsWallLeftEnd", true);
                nearWallEndPos = other.GetComponent<WallEnd>().nearWallEndPos;
                wallEndToEndPos = other.GetComponent<WallEnd>().wallEndToEndPos;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Floor"))
                _isOnFloor = true;

            if (other.CompareTag("Wall"))
            {
                _isWallClose = true;
                wallTransform = other.transform;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Floor"))
                _isOnFloor = false;

            if (other.CompareTag("Wall"))
                _isWallClose = false;

            if (other.CompareTag("WallRightEnd"))
            {
                animator.SetBool("IsWallRightEnd", false);
            }

            if (other.CompareTag("WallLeftEnd"))
            {
                animator.SetBool("IsWallLeftEnd", false);
            }
        }

        #endregion

        #region Private Methods

        void ControlPlayerAnimation()
        {
            switch (unit.curUnitPose)
            {
                #region MOD_WALK
                case Unit.UnitPose.MOD_WALK:
                    //MOD_WALK -> MOD_RUN
                    if (Input.GetButtonDown("Walk"))
                    {
                        animator.SetBool("IsRunMode", true);
                        unit.curUnitPose = Unit.UnitPose.MOD_RUN;

                        break;
                    }

                    //MOD_WALK -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", true);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, true));
                        animator.SetLayerWeight(2, 1);
                        animator.SetLayerWeight(3, 0);
                        unit.curUnitPose = Unit.UnitPose.MOD_COVERSTAND;

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
                            unit.curUnitPose = Unit.UnitPose.MOD_CROUCH;

                        animator.SetLayerWeight(1, animLayerWeight);
                    }

                    break;
                #endregion

                #region MOD_RUN
                case Unit.UnitPose.MOD_RUN:
                    //MOD_WALK -> MOD_RUN
                    if (Input.GetButtonDown("Walk"))
                    {
                        animator.SetBool("IsRunMode", false);
                        unit.curUnitPose = Unit.UnitPose.MOD_WALK;

                        break;
                    }

                    //MOD_RUN -> MOD_COVERSTAND
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", true);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, true));
                        animator.SetLayerWeight(2, 1);
                        animator.SetLayerWeight(3, 0);
                        unit.curUnitPose = Unit.UnitPose.MOD_COVERSTAND;

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
                            unit.curUnitPose = Unit.UnitPose.MOD_CROUCH;

                        animator.SetLayerWeight(1, animLayerWeight);

                        break;
                    }
                    //Debug.Log(unit.curUnitPose);
                    break;
                #endregion

                #region MOD_CROUCH
                case Unit.UnitPose.MOD_CROUCH:
                    //MOD_CROUCH -> MOD_COVERCROUCH
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", true);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, true));
                        animator.SetLayerWeight(2, 0);
                        animator.SetLayerWeight(3, 1);
                        unit.curUnitPose = Unit.UnitPose.MOD_COVERCROUCH;

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
                            unit.curUnitPose = Unit.UnitPose.MOD_RUN;

                        animator.SetLayerWeight(1, animLayerWeight);

                        break;
                    }
                    break;
                #endregion

                #region MOD_COVERSTAND
                case Unit.UnitPose.MOD_COVERSTAND:
                    //MOD_COVERSTAND -> MOD_RUN
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", false);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, false));
                        animator.SetLayerWeight(2, 0);
                        unit.curUnitPose = Unit.UnitPose.MOD_RUN;

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
                            unit.curUnitPose = Unit.UnitPose.MOD_COVERCROUCH;

                        animator.SetLayerWeight(1, animLayerWeight);
                        animator.SetLayerWeight(2, 1 - animLayerWeight);
                        animator.SetLayerWeight(3, animLayerWeight);

                        break;
                    }
                    break;
                #endregion

                #region MOD_COVERCROUCH
                case Unit.UnitPose.MOD_COVERCROUCH:
                    //MOD_COVERCROUCH -> MOD_CROUCH
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", false);
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, false));
                        animator.SetLayerWeight(1, 1);
                        animator.SetLayerWeight(2, 0);
                        animator.SetLayerWeight(3, 0);
                        unit.curUnitPose = Unit.UnitPose.MOD_CROUCH;

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
                            unit.curUnitPose = Unit.UnitPose.MOD_COVERSTAND;

                        animator.SetLayerWeight(1, animLayerWeight);
                        animator.SetLayerWeight(2, 1 - animLayerWeight);
                        animator.SetLayerWeight(3, animLayerWeight);

                        break;
                    }
                    break;
                #endregion

                case Unit.UnitPose.MOD_THROW:
                    break;
                default:
                    break;
            }

            if (_isOnFloor)
            {
                #region Control Move Animation

                if(animator.GetBool("IsFalling"))
                    animator.SetBool("IsFalling", false);

                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing") && unit.curUnitPose != Unit.UnitPose.max)
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

                #endregion

                #region Control Cover Animation

                if (_isWallClose)
                {
                    // 양수면 오른쪽, 음수면 왼쪽
                    animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));

                    if (animator.GetFloat("TurnRight") > 0)
                        animator.SetBool("LookRight", true);
                    else if(animator.GetFloat("TurnRight") < 0)
                        animator.SetBool("LookRight", false);
                }

                #endregion
            }
            else
            {
                animator.SetBool("IsFalling", true);
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

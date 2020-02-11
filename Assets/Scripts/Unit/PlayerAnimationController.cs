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

        #endregion

        #region Public Values

        public bool isOnFloor { get { return _isOnFloor; } }
        public bool isWallClose { get { return _isWallClose; } }

        [HideInInspector]
        public Transform wallTransform;
        [HideInInspector]
        public Vector3 nearWallEndPos;

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
            }

            if (other.CompareTag("WallLeftEnd") && _isWallClose)
            {
                animator.SetBool("IsWallLeftEnd", true);
                nearWallEndPos = other.GetComponent<WallEnd>().nearWallEndPos;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Floor"))
                _isOnFloor = true;
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
                nearWallEndPos = Vector3.zero;
            }

            if (other.CompareTag("WallLeftEnd"))
            {
                animator.SetBool("IsWallLeftEnd", false);
                nearWallEndPos = Vector3.zero;
            }
        }

        #endregion

        #region Private Methods

        void ControlPlayerAnimation()
        {
            #region Control Crouch Animation

            if (Input.GetButtonDown("Crouch"))
            {
                if (animator.GetLayerWeight(1) == 0 || animator.GetLayerWeight(1) == 1)
                {
                    animator.SetLayerWeight(1, animLayerWeight);
                    animator.SetBool("IsCrouchMode", !animator.GetBool("IsCrouchMode"));
                }
            }

            if (animator.GetLayerWeight(1) < 1 && animator.GetBool("IsCrouchMode"))
            {
                animator.SetBool("IsRunMode", false);

                animLayerWeight += (Time.deltaTime * 3f);
                animLayerWeight = Mathf.Clamp01(animLayerWeight);
                animator.SetLayerWeight(1, animLayerWeight);
            }
            else if (animator.GetLayerWeight(1) > 0 && !animator.GetBool("IsCrouchMode"))
            {
                animator.SetBool("IsRunMode", true);

                animLayerWeight -= (Time.deltaTime * 3f);
                animLayerWeight = Mathf.Clamp01(animLayerWeight);
                animator.SetLayerWeight(1, animLayerWeight);
            }

            #endregion

            if (Input.GetButtonDown("Walk") && !animator.GetBool("IsCrouchMode") && animator.GetBool("IsRunMode"))
                animator.SetBool("IsRunMode", false);
            else if (Input.GetButtonDown("Walk") && !animator.GetBool("IsCrouchMode") && !animator.GetBool("IsRunMode"))
                animator.SetBool("IsRunMode", true);


            if (_isOnFloor)
            {
                #region Control Move Animation

                if(animator.GetBool("IsFalling"))
                    animator.SetBool("IsFalling", false);

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

                #endregion

                #region Control Cover Animation

                if (_isWallClose)
                {
                    //엄폐 시작
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", !animator.GetBool("IsCovering"));
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, animator.GetBool("IsCovering")));
                    }

                    //숙인 상태가 아니면 Standing Cover Layer를 활성화 한다.
                    if (!animator.GetBool("IsCrouchMode"))
                    {
                        if (animator.GetBool("IsCovering"))
                        {
                            animator.SetLayerWeight(2, 1);
                            animator.SetLayerWeight(3, 0);
                        }
                        else
                            animator.SetLayerWeight(2, 0);
                    }
                    //숙인 상태면 Crouching Cover Layer를 활성화 한다.
                    else
                    {
                        if (animator.GetBool("IsCovering"))
                        {
                            animator.SetLayerWeight(3, 1);
                            animator.SetLayerWeight(2, 0);
                            animator.SetLayerWeight(1, 0);
                        }
                        else
                            animator.SetLayerWeight(3, 0);
                    }
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

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
                animator.SetBool("IsWallRightEnd", true);

            if (other.CompareTag("WallLeftEnd") && _isWallClose)
                animator.SetBool("IsWallLeftEnd", true);
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
                animator.SetBool("IsWallRightEnd", false);

            if (other.CompareTag("WallLeftEnd"))
                animator.SetBool("IsWallLeftEnd", false);
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

            if (Input.GetButtonDown("Walk") && !animator.GetBool("IsCrouchMode"))
                animator.SetBool("IsRunMode", !animator.GetBool("IsRunMode"));

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
                    if (Input.GetButtonDown("Covering"))
                    {
                        animator.SetBool("IsCovering", !animator.GetBool("IsCovering"));
                        //벽에 붙이기
                        StartCoroutine(unit.SetCoverPosition(wallTransform.position, wallTransform.right, animator.GetBool("IsCovering")));
                    }
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

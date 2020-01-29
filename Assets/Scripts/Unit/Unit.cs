using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{

    public class Unit : MonoBehaviour
    {
        #region Private Values

        private float _speed;
        private uint _health;
        private bool _isOnFloor = false;

        private float animLayerWeight = 0f;

        #endregion

        #region Public Values

        public float speed { get { return _speed; } set { _speed = value; } }
        public float walkSpeed { get { return 0.5f * _speed; } }
        public uint health { get { return _health; } }
        public bool isOnFloor { get { return _isOnFloor; } }

        public Animator animator;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            _health = 1;
            _speed = 30.0f;
        }

        // Start is called before the first frame update
        void Start()
        {
            animator.SetBool("IsRunMode", true);
            animator.SetLayerWeight(1, 0);
            animator.SetBool("IsCrouchMode", false);
        }

        void FixedUpdate()
        {
            ControlAnimation();
        }

        void OnTriggerStay(Collider other)
        {
            if (other.tag.CompareTo("Floor") == 0)
                _isOnFloor = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.tag.CompareTo("Floor") == 0)
                _isOnFloor = false;
        }

        #endregion

        #region Private Methods

        void ControlAnimation()
        {
            #region Control Crouch

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
                animator.SetBool("IsFalling", false);

                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
                {
                    if (Input.GetButton("Vertical"))
                    {
                        animator.SetBool("IsMoving", true);
                        animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Vertical")));
                    }
                    else if (Input.GetButton("Horizontal"))
                    {
                        animator.SetBool("IsMoving", true);
                        animator.SetFloat("MoveSpeed", Mathf.Abs(Input.GetAxis("Horizontal")));
                    }
                    else
                    {
                        animator.SetBool("IsMoving", false);
                        animator.SetFloat("TurnRight", 0);
                        animator.SetFloat("MoveSpeed", 0);
                    }
                }
            }
            else
            {
                animator.SetBool("IsFalling", true);
                animator.SetBool("IsMoving", false);
                animator.SetFloat("TurnRight", 0);
                animator.SetFloat("MoveSpeed", 0);
            }
        }

        #endregion

        #region Public Methods

        public void HitHealth(uint damage)
        {
            if (damage >= _health)
                _health = 0;
            else
                _health -= damage;
        }

        #endregion
    }
}

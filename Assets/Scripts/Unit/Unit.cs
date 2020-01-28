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

        #endregion

        #region Public Values

        public float speed { get { return _speed; } set { _speed = value; } }
        public uint health { get { return _health; } }
        public bool isOnFloor { get { return _isOnFloor; } }

        public Animator animator;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            _health = 1;
            _speed = 50.0f;
        }

        // Start is called before the first frame update
        void Start()
        {
            animator.SetBool("IsRunMode", true);
        }

        void FixedUpdate()
        {
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

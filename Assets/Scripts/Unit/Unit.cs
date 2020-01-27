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

        }

        void FixedUpdate()
        {
            if (isOnFloor)
            {
                if (Input.GetAxis("Vertical") != 0)
                {
                    Debug.Log("V: " + Input.GetAxis("Vertical"));
                    animator.SetFloat("movespeed", Mathf.Abs(Input.GetAxis("Vertical")));
                    animator.SetFloat("TurnRight", Input.GetAxis("Vertical"));
                }
                else if (Input.GetAxis("Horizontal") != 0)
                {
                    Debug.Log("H: " + Input.GetAxis("Horizontal"));
                    animator.SetFloat("movespeed", Mathf.Abs(Input.GetAxis("Horizontal")));
                    animator.SetFloat("TurnRight", Input.GetAxis("Horizontal"));
                }
                else
                {
                    animator.SetFloat("movespeed", 0);
                    animator.SetFloat("TurnRight", 0);
                }

                Debug.Log("WTF with TurnRight: " + animator.GetFloat("TurnRight"));
            }
            else
            {
                animator.SetFloat("movespeed", 0);
                animator.SetFloat("TurnRight", 0);
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

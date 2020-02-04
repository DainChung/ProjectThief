﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerController : MonoBehaviour
    {
        #region Private Vars

        private float playerSpeed;

        private Transform mainCameraTransform;
        private Quaternion destiRotation;
        private Vector3 lookDir;
        private Vector3 destiPos;

        private Rigidbody rb;

        private Animator animator;

        const float crouchColliderHeight = 64f;
        const float standColliderHeight = 90f;

        //높을수록 적캐릭터에게 쉽게 들킨다.
        private float aggro = 0.1f;

        private float jumpPower = 10f;

        private Unit unit;
        private PlayerAnimationController playerAnimController;

        #endregion

        #region Public Vars

        #endregion

        #region MonoBehaviour Callback

        void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            unit = GetComponent<Unit>();
            playerAnimController = GetComponent<PlayerAnimationController>();
            rb = GetComponent<Rigidbody>();
            animator = playerAnimController.animator;

            playerSpeed = unit.speed;

            mainCameraTransform = Camera.main.transform;
            lookDir = mainCameraTransform.forward + transform.position;
        }

        void FixedUpdate()
        {
            #region Control.Crouch: 조작에 따라 속도와 Collider를 관리
            //걷기 <-> 달리기 전환 이거나 서기 <-> 숙이기 전환
            if (Input.GetButtonDown("Walk") || Input.GetButtonDown("Crouch"))
            {
                if (!animator.GetBool("IsCrouchMode"))
                {
                    transform.GetComponent<CapsuleCollider>().height = standColliderHeight;
                    transform.GetComponent<CapsuleCollider>().center = new Vector3(0, standColliderHeight / 2, 0);

                    if (animator.GetBool("IsRunMode"))
                        playerSpeed = unit.speed;
                    else
                        playerSpeed = unit.walkSpeed;
                }
                else
                {
                    transform.GetComponent<CapsuleCollider>().height = crouchColliderHeight;
                    transform.GetComponent<CapsuleCollider>().center = new Vector3(0, crouchColliderHeight / 2, 0);

                    playerSpeed = unit.walkSpeed;
                }
            }
            #endregion
      
            //바닥 위에 있을 때
            //이후 은신 관련 기능 추가 (예시)))float cloak; if (플레이어 == 서있음) ctraloak = 1; else if (플레이어 == 숙이기) cloak = 0.5;)
            if (playerAnimController.isOnFloor)
            {
                #region Control.Base: 일반적인 상태에서의 조작 관리
                if (!animator.GetBool("IsCovering"))
                {
                    //플레이어 캐릭터 회전 (엄폐 상태가 아닐때만)
                    SetLookDir(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
                    transform.LookAt(lookDir, Vector3.up);
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);  //플레이어가 낙하할 때 x축 또는 z축이 회전하는 현상 방지, freezeRotation으로 제어 안 됨

                    //플레이어 캐릭터 이동
                    if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
                    {
                        destiPos = transform.forward * playerSpeed;

                        rb.AddForce(destiPos);
                    }

                    //점프할 때
                    if (!animator.GetBool("IsCrouchMode") && animator.GetBool("IsRunMode"))
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            destiPos = transform.up * playerSpeed * jumpPower;

                            rb.AddForce(destiPos);
                        }
                    }
                }
                #endregion

                #region Control.Cover: 엄폐상태에서의 조작 관리
                //벽 같은 엄폐물에 엄폐했을 때 Vector3값 일부분과 바라보는 방향 고정
                if (playerAnimController.isWallClose && animator.GetBool("IsCovering"))
                {
                    //플레이어 캐릭터 방향 고정
                    transform.LookAt(transform.position + playerAnimController.wallTransform.forward, Vector3.up);

                    //엄폐 상태일 때 가능한 조작 사용
                }
                #endregion

                //과도한 미끄러짐 방지
                rb.velocity *= 0.97f;
            }


            //플레이어가 의도하지 않은 회전 방지           
            rb.angularVelocity = Vector3.zero;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("EndArea"))
                Debug.Log("Escape");
        }

        #endregion

        #region Private Methods

        //카메라가 보고 있는 방향을 전방으로 정함
        void SetLookDir(float vertical, float horizontal)
        {
            lookDir = (vertical * mainCameraTransform.forward) + (horizontal * mainCameraTransform.right) + transform.position;
            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
        }

        #endregion
    }
}

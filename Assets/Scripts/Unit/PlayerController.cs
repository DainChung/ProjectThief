using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerController : MonoBehaviour
    {
        #region Private Vars

        private float playerSpeed;
        private float walkSpeed;

        private Transform mainCameraTransform;
        private Quaternion destiRotation;
        private Vector3 lookDir;
        private Vector3 destiPos;

        private Rigidbody rb;

        private Animator animator;

        //높을수록 적캐릭터에게 쉽게 들킨다.
        private float aggro = 0.1f;

        #endregion

        #region Public Vars
        [HideInInspector]
        public Unit unit;

        #endregion

        #region MonoBehaviour Callback

        void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            unit = GetComponent<Unit>();
            rb = GetComponent<Rigidbody>();
            animator = unit.animator;

            playerSpeed = unit.speed;
            walkSpeed = playerSpeed * 0.5f;

            mainCameraTransform = Camera.main.transform;
            lookDir = mainCameraTransform.forward + transform.position;
        }

        void FixedUpdate()
        {
            ControlSpeed();

            //바닥과 접촉했을 때만 가능
            //숙이기 구현 -> 플레이어 이동속도 감소
            //이후 은신 관련 기능 추가 (예시)))float cloak; if (플레이어 == 서있음) ctraloak = 1; else if (플레이어 == 숙이기) cloak = 0.5;)
            if (unit.isOnFloor)
            {
                //플레이어 캐릭터 회전
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
                        destiPos = transform.up * playerSpeed * 20;

                        rb.AddForce(destiPos);
                    }
                }

                //과도한 미끄러짐 방지
                rb.velocity *= 0.97f;
            }
            //플레이어가 의도하지 않은 회전 방지           
            rb.angularVelocity = Vector3.zero;
        }

        #endregion

        #region Private Methods

        //카메라가 보고 있는 방향을 전방으로 정함
        void SetLookDir(float vertical, float horizontal)
        {
            lookDir = (vertical * mainCameraTransform.forward) + (horizontal * mainCameraTransform.right) + transform.position;
            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
        }

        //조작에 따라 이동속도를 조절하는 함수
        void ControlSpeed()
        {
            //걷기 <-> 달리기 전환 이거나 서기 <-> 숙이기 전환
            if (Input.GetButtonDown("Walk") || Input.GetButtonDown("Crouch"))
            {
                if (!animator.GetBool("IsCrouchMode"))
                {
                    if (animator.GetBool("IsRunMode"))
                        playerSpeed = unit.speed;
                    else
                        playerSpeed = unit.walkSpeed;
                }
                else
                {
                    playerSpeed = unit.walkSpeed;
                }
            }
        }

        #endregion
    }
}

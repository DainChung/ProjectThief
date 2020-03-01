using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{ 
    public class PlayerController : MonoBehaviour
    {
        #region Private Vars

        private enum LookDirState
        {
            IDLE = 0, COVER, THROW, max
        }

        private LookDirState curLookDirState = LookDirState.IDLE;

        private float playerSpeed;
        private Transform mainCameraTransform;
        private Quaternion destiRotation;
        private Vector3 lookDir;
        private Vector3 destiPos;

        private Rigidbody rb;

        private Animator animator;

        //높을수록 적캐릭터에게 쉽게 들킨다.
        private float aggro = 0.1f;

        private Unit unit;
        private PlayerAnimationController playerAnimController;

        private WeaponCode curWeapon = WeaponCode.HAND;

        //조준 후 발사되지 않는 경우 발사하도록 제어
        private bool readyToThrowItem = false;
        //동시에 두번 발사 되는 현상 방지
        private bool doubleThrowLock = false;

        private Vector3 throwRotEuler = Vector3.zero;
        private CameraWork cam;

        //무기 딜레이
        private Stopwatch[] attackSW = new Stopwatch[4];
        private int[] attackDelayTime = new int[4];

        private Stopwatch throwAnimSW = new Stopwatch();

        #endregion

        #region Public Vars

        public Transform throwPos;

        #endregion

        #region MonoBehaviour Callback

        void Awake()
        {
            attackDelayTime[0] = 1;
            attackDelayTime[1] = 2;
            attackDelayTime[2] = 3;
            attackDelayTime[3] = 4;
        }

        // Start is called before the first frame update
        void Start()
        {
            unit = GetComponent<Unit>();
            playerAnimController = GetComponent<PlayerAnimationController>();
            rb = GetComponent<Rigidbody>();
            animator = playerAnimController.animator;

            mainCameraTransform = Camera.main.transform;
            cam = mainCameraTransform.GetComponent<CameraWork>();
            lookDir = mainCameraTransform.forward + transform.position;

            playerSpeed = unit.speed;

            attackSW[0] = new Stopwatch();
            attackSW[1] = new Stopwatch();
            attackSW[2] = new Stopwatch();
            attackSW[3] = new Stopwatch();

            attackSW[0].Start();
            attackSW[1].Start();
            attackSW[2].Start();
            attackSW[3].Start();

            throwAnimSW.Start();
        }

        void FixedUpdate()
        {
            //움직임이 허용된 상태에서만 조작 가능
            if (!unit.lockControl)
            {
                #region Control.Weapon: 조작에 따라 무기 변경

                if (Input.GetButtonDown("Weapon1"))
                {
                    curWeapon = WeaponCode.HAND;
                    UnityEngine.Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon2"))
                {
                    curWeapon = WeaponCode.CAN;
                    UnityEngine.Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon3"))
                {
                    curWeapon = WeaponCode.DONUT;
                    UnityEngine.Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon4"))
                {
                    curWeapon = WeaponCode.SMOKE;
                    UnityEngine.Debug.Log("CurWeapon: " + curWeapon);
                }

                #endregion

                //ControlCover(), ControlBase()에 나누어 넣을 예정
                #region Control.Crouch: 조작에 따라 속도와 Collider를 관리
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
                #endregion

                //바닥 위에 있을 때
                if (playerAnimController.isOnFloor)
                {
                    #region Control.Move & AttackThrow
                    switch (unit.curUnitPose)
                    {
                        case UnitPose.MOD_WALK:
                            ControlBase();
                            break;
                        case UnitPose.MOD_RUN:
                            ControlBase();
                            break;
                        case UnitPose.MOD_CROUCH:
                            ControlBase();
                            break;
                        case UnitPose.MOD_COVERSTAND:
                        case UnitPose.MOD_COVERCROUCH:
                            ControlCover();
                            break;
                        case UnitPose.MOD_THROW:
                            ControlMoveThrow();
                            break;
                        default:
                            break;
                    }

                    //바닥에 서있을 때의 공격 관련 조작 관리
                    ControlAttack();

                    //과도한 미끄러짐 방지
                    rb.velocity *= 0.97f;
                    #endregion
                }
                else
                {
                    animator.SetLayerWeight(4, 0);
                    animator.SetLayerWeight(5, 0);
                    cam.HideLines();
                }

            }

            LookDir();
            //플레이어가 의도하지 않은 회전 방지           
            rb.angularVelocity = Vector3.zero;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("EndArea"))
                UnityEngine.Debug.Log("Escape");
        }

        #endregion

        #region Private Methods

        //카메라가 보고 있는 방향을 전방으로 정함
        void SetLookDir(float vertical, float horizontal)
        {
            lookDir = (vertical * mainCameraTransform.forward) + (horizontal * mainCameraTransform.right) + transform.position;
            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
        }

        //플레이어 캐릭터가 특정 방향을 바라보게 하는 함수
        void LookDir()
        {
            switch (curLookDirState)
            {
                case LookDirState.IDLE:
                    SetLookDir(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
                    break;
                case LookDirState.COVER:
                    lookDir = transform.position + playerAnimController.wallTransform.forward;
                    break;
                case LookDirState.THROW:
                    lookDir = transform.position + mainCameraTransform.forward;
                    break;
                default:
                    break;
            }

            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
            transform.LookAt(lookDir, Vector3.up);
            //플레이어가 낙하할 때 x축 또는 z축이 회전하는 현상 방지, freezeRotation으로 제어 안 됨
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        private void AttackDelayManager()
        {
            for (int i = 0; i < attackSW.Length; i++)
            {
                if (attackSW[i].IsRunning)
                {
                    if (attackSW[i].Elapsed.Seconds >= attackDelayTime[i])
                        attackSW[i].Stop();
                }
            }     
        }

        private bool AttackDelayDone(WeaponCode code)
        {
            return attackSW[(int)(code)].Elapsed.Seconds >= attackDelayTime[(int)(code)];
        }

        //마우스 좌측을 길게 눌러서 조준후 발사 => 소음을 발생시켜 주의분산
        //소지 개수 이상으로 사용불가(예정)
        private void AttackThrow()
        {
            //특정 상태일 때만 투척 허용
            //달리기, 걷기, 던지기, 투척 직후
            switch (unit.curUnitPose)
            {
                case UnitPose.MOD_RUN:
                case UnitPose.MOD_WALK:
                case UnitPose.MOD_THROW:
                case UnitPose.MOD_THROWEND:
                    #region 조준 및 투척
                    //if(amountCAN > 0)
                    if (AttackDelayDone(curWeapon))
                    {
                        //조준
                        if (Input.GetButton("Fire1"))
                            AttackAimingPhase();
                        //조준한 상태에서 놓으면 투척
                        else if ((Input.GetButtonUp("Fire1") || readyToThrowItem) && doubleThrowLock)
                            AttackThrowPhase();
                    }
                    //Throw 애니메이션이 재생 중
                    else if (animator.GetBool("ThrowItem"))
                    {
                        //이동방지
                        unit.curUnitPose = UnitPose.MOD_THROWEND;
                        animator.SetLayerWeight(4, animator.GetLayerWeight(4) - Time.deltaTime);
                    }

                    //Throw 애니메이션 종료 후
                    if (animator.GetLayerWeight(4) <= 0 && animator.GetBool("ThrowItem"))
                    {
                        curLookDirState = LookDirState.IDLE;
                        unit.curUnitPose = UnitPose.MOD_RUN;

                        animator.Play("Throw", 4, 0.0f);
                        animator.SetFloat("ThrowAnimSpeed", 0);
                        animator.SetLayerWeight(4, 0);
                        animator.SetLayerWeight(5, 0);
                        animator.SetBool("ThrowItem", false);

                        throwAnimSW.Stop();
                    }
                    //else
                    //Debug.Log("소지 개수 부족");
                    #endregion
                    break;
                default:
                    break;
            }
        }

        //Fire 1 키를 누르고 있으면 조준
        //GetButton("Fire1") => 조준
        private void AttackAimingPhase()
        {
            float theta;

            animator.SetBool("IsThrowMode", true);
            curLookDirState = LookDirState.THROW;
            unit.curUnitPose = UnitPose.MOD_THROW;

            //발사각 결정
            throwRotEuler = mainCameraTransform.rotation.eulerAngles;
            theta = throwRotEuler.x;

            if (theta >= 334)
                theta = theta - 360;

            theta = theta - 35;

            throwRotEuler.Set(theta, throwRotEuler.y, 0);

            //포물선 궤적 그리기
            cam.ThrowLineRenderer(throwRotEuler.x, throwPos.position);

            //플레이어 캐릭터 속도 & 회전 & 애니메이션 관리
            playerSpeed = unit.walkSpeed;

            animator.SetLayerWeight(4, 1);
            animator.SetFloat("ThrowAnimSpeed", 0.01f);
            if (throwAnimSW.Elapsed.Seconds >= 1)
            {
                animator.Play("Throw", 4, 0);
                throwAnimSW.Restart();
            }
            //animator.SetFloat("ThrowAnimSpeed", 0);

            animator.SetBool("IsRunMode", false);

            readyToThrowItem = true;
            doubleThrowLock = true;
        }

        //Fire 1 키를 떼면 투척
        //GetButtonUp("Fire1") => 투척
        private void AttackThrowPhase()
        {
            animator.SetBool("IsThrowMode", false);
            curLookDirState = LookDirState.THROW;

            animator.SetLayerWeight(4, 1);
            animator.SetLayerWeight(5, 0);
            animator.SetFloat("ThrowAnimSpeed", 1);

            Instantiate(Resources.Load(unit.weaponPath + curWeapon.ToString()) as GameObject, throwPos.position, Quaternion.Euler(throwRotEuler));

            cam.HideLines();
            if (!animator.GetBool("IsCovering"))
            {
                playerSpeed = unit.speed;
                animator.SetBool("IsRunMode", true);
            }

            animator.SetBool("ThrowItem", true);
            readyToThrowItem = false;
            doubleThrowLock = false;

            //타이머 작동
            attackSW[(int)curWeapon].Restart();
        }

        //일반적인 상태에서의 조작
        private void ControlBase()
        {
            curLookDirState = LookDirState.IDLE;

            //플레이어 캐릭터 이동
            if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
            {
                destiPos = transform.forward * playerSpeed;

                rb.AddForce(destiPos);
            }

            //점프할 때
            if (animator.GetBool("IsRunMode"))
            {
                if (Input.GetButtonDown("Jump"))
                {
                    destiPos = transform.up * unit.speed * unit.jumpPower;

                    rb.AddForce(destiPos);
                }
            }
        }
        //조준 상태일 때의 조작
        private void ControlMoveThrow()
        {
            //플레이어 캐릭터 이동
            if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
            {
                destiPos = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * playerSpeed;

                rb.AddForce(destiPos);
            }
        }
        //엄폐 상태일 때의 조작
        //플레이어 속도는 unit.coverSpeed로 고정됨
        private void ControlCover()
        {
            //벽 같은 엄폐물에 엄폐했을 때 Vector3값 일부분과 바라보는 방향 고정

            curLookDirState = LookDirState.COVER;

            //엄폐 상태일 때 가능한 조작 사용
            if (Input.GetButton("Horizontal"))
            {
                //벽 우측 끝 도달 && 우측으로 계속 이동하려는 경우
                if (animator.GetBool("IsWallRightEnd") && Input.GetAxis("Horizontal") > 0)
                {
                    //아무것도 하지 않음
                }
                //벽 좌측 끝 도달 && 좌측으로 계속 이동하려는 경우
                else if (animator.GetBool("IsWallLeftEnd") && Input.GetAxis("Horizontal") < 0)
                {
                    //아무것도 하지 않음
                }
                //일반적인 상황 OR 벽 우측 끝에서 좌측으로 이동 OR 벽 좌측 끝에서 우측으로 이동
                else
                {
                    destiPos = -transform.right * Input.GetAxis("Horizontal") * unit.coverSpeed;

                    rb.AddForce(destiPos);
                }
            }

            //우측 끝에서 우측 이동 버튼을 다시 누르면 엄폐물 이동 수행
            if (animator.GetBool("IsWallRightEnd"))
            {
                if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") > 0)
                    StartCoroutine(unit.SetCoverPosition(playerAnimController.nearWallEndPos, true, playerAnimController.wallEndToEndPos));
            }
            //좌측 끝에서 좌측 이동 버튼을 다시 누르면 엄폐물 이동 수행
            else if (animator.GetBool("IsWallLeftEnd"))
            {
                if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") < 0)
                    StartCoroutine(unit.SetCoverPosition(playerAnimController.nearWallEndPos, false, playerAnimController.wallEndToEndPos));
            }
        }
        //공격에 관한 조작
        private void ControlAttack()
        {
            AttackDelayManager();

            switch (curWeapon)
            {
                //비무장, 아무것도 할 수 없음
                case WeaponCode.HAND:
                    break;
                case WeaponCode.CAN:
                    AttackThrow();
                    break;
                //플레이어 주변에 설치 OR 던지기?
                case WeaponCode.DONUT:
                    AttackThrow();
                    break;
                //플레이어 위치에서 사용
                case WeaponCode.SMOKE:
                    if (Input.GetButtonDown("Fire1") && AttackDelayDone(curWeapon))
                    {
                        Instantiate(Resources.Load(unit.weaponPath + curWeapon.ToString()) as GameObject,
                                                    transform.position + TransformCollections.weaponSmokeVec,
                                                    TransformCollections.weaponSmokeQuat);
                        attackSW[(int)curWeapon].Restart();
                    }
                    break;
                default:
                    break;
            }
        }
        //아이템을 주울때
        private void ControlGetItem()
        {
            /*
             if(Input.GetButton("GetItem"))
             {
                
             }
             */
        }
        #endregion
    }
}

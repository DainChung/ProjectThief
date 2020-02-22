using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

namespace Com.MyCompany.MyGame
{
    enum WeaponCode
    {
        HAND, CAN, DONUT, SMOKE
    }

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
        private float theta;
        private CameraWork cam;

        //무기 딜레이
        private Stopwatch[] attackSW = new Stopwatch[4];
        private int[] attackDelayTime = new int[4];

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
                //이후 은신 관련 기능 추가 (예시)))float cloak; if (플레이어 == 서있음) ctraloak = 1; else if (플레이어 == 숙이기) cloak = 0.5;)
                if (playerAnimController.isOnFloor)
                {
                    switch (unit.curUnitPose)
                    {
                        case Unit.UnitPose.MOD_WALK:
                        case Unit.UnitPose.MOD_RUN:
                        case Unit.UnitPose.MOD_CROUCH:
                            ControlBase();
                            break;
                        case Unit.UnitPose.MOD_COVERSTAND:
                        case Unit.UnitPose.MOD_COVERCROUCH:
                            ControlCover();
                            break;
                        default:
                            break;
                    }

                    //바닥에 서있을 때의 공격 관련 조작 관리
                    ControlAttack();

                    //과도한 미끄러짐 방지
                    rb.velocity *= 0.97f;
                }

            }

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

        void ControlAttack()
        {
            AttackDelayManager();

            switch (curWeapon)
            {
                //근접 공격용 맨손 (=> 일시적 경직 ?)
                case WeaponCode.HAND:
                    if (Input.GetButtonDown("Fire1"))
                        UnityEngine.Debug.Log("손 공격");
                    break;
                case WeaponCode.CAN:
                    AttackThrowCan();
                    break;
                //플레이어 주변에 설치 OR 던지기?
                case WeaponCode.DONUT:
                    if (Input.GetButtonDown("Fire1"))
                        UnityEngine.Debug.Log("도넛 설치");
                    break;
                //플레이어 위치에서 사용
                case WeaponCode.SMOKE:
                    if (Input.GetButtonDown("Fire1"))
                        UnityEngine.Debug.Log("연막");
                    break;
                default:
                    break;
            }
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
        //소지 개수 이상으로 사용불가
        //조준 상태에서 조준 방향과 다른 쪽으로 이동할 때 캐릭터의 방향이 부자연스럽게 변경되는 오류 있음
        //=> 조준 동안에는 조준점을 보지만 던질 때 이동방향을 바라봄
        private void AttackThrowCan()
        {
            //if(amountCAN > 0)
            //조준
            if (AttackDelayDone(curWeapon))
            {
                if (Input.GetButton("Fire1"))
                {
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
                    if (!animator.GetBool("IsCovering"))
                    {
                        playerSpeed = unit.walkSpeed;

                        lookDir = transform.position + mainCameraTransform.forward;
                        lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
                        transform.LookAt(lookDir, Vector3.up);
                    }

                    animator.SetFloat("ThrowAnimSpeed", 0);

                    animator.SetBool("ReadyToThrow", true);
                    animator.SetBool("IsRunMode", false);

                    readyToThrowItem = true;
                    doubleThrowLock = true;
                }
                //조준한 상태에서 놓으면 투척
                else if ((Input.GetButtonUp("Fire1") || readyToThrowItem) && doubleThrowLock)
                {
                    animator.SetLayerWeight(4, 1);
                    animator.SetFloat("ThrowAnimSpeed", 1);

                    Instantiate(Resources.Load(unit.weaponPath + "WeaponCan") as GameObject, throwPos.position, Quaternion.Euler(throwRotEuler));

                    cam.HideLines();
                    if (!animator.GetBool("IsCovering"))
                    {
                        playerSpeed = unit.speed;
                        animator.SetBool("IsRunMode", true);
                    }

                    animator.SetBool("ReadyToThrow", false);
                    animator.SetBool("ThrowItem", true);
                    readyToThrowItem = false;
                    doubleThrowLock = false;

                    //타이머 작동
                    attackSW[(int)(WeaponCode.CAN)].Restart();
                }
                //else
                //Debug.Log("소지 개수 부족");
            }
            else
            {
                animator.SetLayerWeight(4, animator.GetLayerWeight(4) - Time.deltaTime);
            }

            if (animator.GetLayerWeight(4) <= 0)
            {
                animator.Play("Throw", 4, 0.0f);
                animator.SetFloat("ThrowAnimSpeed", 0);
                animator.SetLayerWeight(4, 0);
                animator.SetBool("ThrowItem", false);
            }
        }

        //일반적인 상태에서의 조작
        private void ControlBase()
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
            if (animator.GetBool("IsRunMode"))
            {
                if (Input.GetButtonDown("Jump"))
                {
                    destiPos = transform.up * unit.speed * unit.jumpPower;

                    rb.AddForce(destiPos);
                }
            }
        }

        private void ControlCover()
        {
            //벽 같은 엄폐물에 엄폐했을 때 Vector3값 일부분과 바라보는 방향 고정
            //플레이어 속도는 unit.coverSpeed로 고정됨
            //플레이어 캐릭터 방향 고정
            transform.LookAt(transform.position + playerAnimController.wallTransform.forward, Vector3.up);

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

        #endregion
    }
}

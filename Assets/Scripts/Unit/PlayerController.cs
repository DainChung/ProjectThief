using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private Vector3 throwRotEuler = Vector3.zero;
        private float theta;
        private CameraWork cam;

        //System.Diagnostic
        //무기 딜레이
        //private Stopwatch[] sw = new Stopwatch[5];

        #endregion

        #region Public Vars

        public Transform throwPos;

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

            mainCameraTransform = Camera.main.transform;
            cam = mainCameraTransform.GetComponent<CameraWork>();
            lookDir = mainCameraTransform.forward + transform.position;

            playerSpeed = unit.speed;
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
                    Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon2"))
                {
                    curWeapon = WeaponCode.CAN;
                    Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon3"))
                {
                    curWeapon = WeaponCode.DONUT;
                    Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon4"))
                {
                    curWeapon = WeaponCode.SMOKE;
                    Debug.Log("CurWeapon: " + curWeapon);
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
                    switch (unit.curUnitState)
                    {
                        case Unit.UnitState.MOD_WALK:
                        case Unit.UnitState.MOD_RUN:
                        case Unit.UnitState.MOD_CROUCH:
                            ControlBase();
                            break;
                        case Unit.UnitState.MOD_COVERSTAND:
                        case Unit.UnitState.MOD_COVERCROUCH:
                            ControlCover();
                            break;
                        default:
                            break;
                    }

                    //ControlCover에 넣을 예정
                    #region Control.Cover: 엄폐상태에서의 조작 관리

                    #endregion

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

        void ControlAttack()
        {
            switch (curWeapon)
            {
                //근접 공격용 맨손 (=> 일시적 경직 ?)
                case WeaponCode.HAND:
                    if (Input.GetButtonDown("Fire1"))
                        Debug.Log("손 공격");
                    break;
                case WeaponCode.CAN:
                    ThrowCan();
                    break;
                //플레이어 주변에 설치 OR 던지기?
                case WeaponCode.DONUT:
                    if (Input.GetButtonDown("Fire1"))
                        Debug.Log("도넛 설치");
                    break;
                //플레이어 위치에서 사용
                case WeaponCode.SMOKE:
                    if (Input.GetButtonDown("Fire1"))
                        Debug.Log("연막");
                    break;
                default:
                    break;
            }
        }

        //마우스 좌측을 길게 눌러서 조준후 발사 => 소음을 발생시켜 주의분산
        //소지 개수 이상으로 사용불가 && 딜레이 추가할 것
        //
        private void ThrowCan()
        {
            //if(amountCAN > 0)
            //조준
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
                animator.SetBool("IsRunMode", false);

                readyToThrowItem = true;
            }
            //조준한 상태에서 놓으면 투척
            else if (Input.GetButtonUp("Fire1") || readyToThrowItem)
            {
                Instantiate(Resources.Load(unit.weaponPath + "WeaponCan") as GameObject, throwPos.position, Quaternion.Euler(throwRotEuler));

                cam.HideLines();
                if (!animator.GetBool("IsCovering"))
                {
                    playerSpeed = unit.speed;
                    animator.SetBool("IsRunMode", true);
                }

                readyToThrowItem = false;
            }
            //else
            //Debug.Log("소지 개수 부족");
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

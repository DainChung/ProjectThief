using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{ 
    public class PlayerController : MonoBehaviour
    {
        #region Sub Classes

        private class Inventory
        {
            private bool _takeGold = false;
            private int[] _items = new int[(int)Item.max - 1];
            private int[] maxAmount = new int[(int)Item.max - 1];

            public bool takeGold { get { return _takeGold; } }
            public int[] items { get { return _items; } }

            public Inventory()
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = 0;
                    maxAmount[i] = ValueCollections.itemMaxAmount[i];
                }
                _takeGold = false;
            }

            public void Add(Item item, int amount)
            {
                switch (item)
                {
                    case Item.CAN:
                    case Item.CHEESE:
                    case Item.SMOKE:

                        int index = (int)(item) - 1;

                        if (amount > 0)
                        {
                            if (_items[index] + amount > maxAmount[index])
                                _items[index] = maxAmount[index];
                            else
                                _items[index] += amount;
                        }
                        break;
                    default:
                        break;
                }

            }
            public void Add(Item item)
            { 
                switch (item)
                {
                    case Item.CAN:
                    case Item.CHEESE:
                    case Item.SMOKE:

                        int index = (int)(item) - 1;

                        if (_items[index] + 1 > maxAmount[index])
                            _items[index] = maxAmount[index];
                        else
                            _items[index]++;
                        break;
                    case Item.GOLD:
                        _takeGold = true;
                        break;
                    default:
                        break;
                }
            }
            public void Add(int index)
            {
                index--;

                if (_items[index] + 1 > maxAmount[index])
                    _items[index] = maxAmount[index];
                else
                    _items[index]++;

                ShowInvnetory();
            }

            public void Remove(Item item, int amount)
            {
                switch (item)
                {
                    case Item.CAN:
                    case Item.CHEESE:
                    case Item.SMOKE:

                        int index = (int)(item) - 1;

                        if (amount > 0)
                        {
                            if (_items[index] - amount < 0)
                                _items[index] = 0;
                            else
                                _items[index] -= amount;
                        }
                        break;
                    default:
                        break;
                }
            }
            public void Remove(Item item)
            {
                switch (item)
                {
                    case Item.CAN:
                    case Item.CHEESE:
                    case Item.SMOKE:

                        int index = (int)(item) - 1;

                        if (_items[index] - 1 < 0)
                            _items[index] = 0;
                        else
                            _items[index]--;
                        break;
                    case Item.GOLD:
                        _takeGold = false;
                        break;
                    default:
                        break;
                }
            }
            public void Remove(int index)
            {
                index--;

                if (_items[index] - 1 < 0)
                    _items[index] = 0;
                else
                    _items[index]--;

                ShowInvnetory();
            }

            public bool CheckWeapon(WeaponCode weapon)
            {
                bool result = false;

                switch (weapon)
                {
                    case WeaponCode.CAN:
                    case WeaponCode.CHEESE:
                    case WeaponCode.SMOKE:
                        result = _items[(int)(weapon) - 1] > 0;
                        break;
                    case WeaponCode.HAND:
                    default:
                        result =  true;
                        break;
                }

                if (!result)
                    UnityEngine.Debug.Log("Not Enough " + weapon.ToString());

                return result;
            }
            public void ShowInvnetory()
            {
                UnityEngine.Debug.Log("[ShowInventory] GOLD: " + _takeGold + ", CAN: " + _items[0] + ", CHEESE: " + _items[1] + ", SMOKE: " + _items[2]);
            }
        }

        #endregion

        #region Private Fields

        private LookDirState curLookDirState = LookDirState.IDLE;

            #region 기본 이동 및 물리
        private float playerSpeed;
        private Transform mainCameraTransform;
        private Quaternion destiRotation;
        private Vector3 lookDir;
        private Vector3 destiPos;

        private Rigidbody rb;
        #endregion

            #region 애니메이션
        private Animator animator;
        private PlayerAnimationController playerAnimController;
        #endregion

            #region 무기 및 공격
        private WeaponCode curWeapon = WeaponCode.HAND;

        private Vector3 throwRotEuler = Vector3.zero;
            #endregion

            #region 어그로 관련 변수
        //높을수록 적캐릭터에게 쉽게 들킨다.
        private float aggroValue;
        
            #endregion

        private Unit unit;
        private CameraWork cam;
        private Inventory pInventory = new Inventory();

        #endregion

        #region Public Fields

        public Transform throwPos;
        public CheckCameraCollider checkCameraCollider;
        [HideInInspector]
        public LookDirState curlookDir { set { curLookDirState = value; } }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            aggroValue = AggroCollections.aggroRun;

            //인벤토리 초기화
            pInventory.Add(Item.CAN, 5);
            pInventory.Add(Item.CHEESE, 3);
            pInventory.Add(Item.SMOKE, 2);
        }

        void Start()
        {
            unit = GetComponent<Unit>();
            playerAnimController = GetComponent<PlayerAnimationController>();
            rb = GetComponent<Rigidbody>();
            animator = unit.animator;

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
                    UnityEngine.Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon2"))
                {
                    curWeapon = WeaponCode.CAN;
                    UnityEngine.Debug.Log("CurWeapon: " + curWeapon);
                }
                else if (Input.GetButtonDown("Weapon3"))
                {
                    curWeapon = WeaponCode.CHEESE;
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
                if (unit.IsOnFloor())
                {
                    #region Control.Move & AttackThrow
                    switch (unit.curUnitPose)
                    {
                        case UnitPose.MOD_WALK:
                            ControlBase();
                            ControlAttack();
                            break;
                        case UnitPose.MOD_RUN:
                            ControlBase();
                            ControlAttack();
                            break;
                        case UnitPose.MOD_CROUCH:
                            ControlBase();
                            ControlAttack();
                            break;
                        case UnitPose.MOD_COVERSTAND:
                            ControlCover();
                            break;
                        case UnitPose.MOD_COVERCROUCH:
                            ControlCover();
                            break;
                        case UnitPose.MOD_THROW:
                            ControlThrowMove();
                            ControlAttack();
                            SetBYCurUnitPose();
                            break;
                        case UnitPose.MOD_THROWEND:
                            ControlAttack();
                            SetBYCurUnitPose();
                            break;
                        case UnitPose.MOD_ATTACK:
                            ControlAttack();
                            break;
                        default:
                            break;
                    }

                    //과도한 미끄러짐 방지
                    rb.velocity *= 0.97f;
                    #endregion
                }

                LookDir();
            }

            //UnityEngine.Debug.Log("Aggro: " + aggro);
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
                    lookDir = transform.position + unit.WallTransform().forward;
                    break;
                case LookDirState.THROW:
                    lookDir = transform.position + mainCameraTransform.forward;
                    break;
                default:
                    lookDir = transform.position + transform.forward;
                    break;
            }

            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
            transform.LookAt(lookDir, Vector3.up);
            //플레이어가 낙하할 때 x축 또는 z축이 회전하는 현상 방지, freezeRotation으로 제어 안 됨
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        /// <summary>
        /// 일반 공격
        /// </summary>
        private void AttackDefault()
        {
            if (unit.swManager.AttackDelayDone(WeaponCode.HAND))
                if (Input.GetButtonDown("Fire1"))
                {
                    MyDebug.Log("AttackDefault");
                    unit.AttackDefault(ref rb);
                    curLookDirState = LookDirState.max;
                }
        }

        /// <summary>
        /// 암살, 비전투 상태에서만 가능
        /// </summary>
        private void AttackAssassinate()
        {
            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                case UnitState.INSMOKE:
                case UnitState.ALERT:

                    if (unit.swManager.AttackDelayDone(WeaponCode.HAND))
                        if (Input.GetButton("Assassinate") && checkCameraCollider.canAssassinate)
                            StartCoroutine(AssassinateMove());
                    else
                        checkCameraCollider.InitAssassinateTargetPos();

                    break;
                default:
                    break;
            }
        }
        private IEnumerator AssassinateMove()
        {
            unit.lockControl = true;
            unit.assassinate.enableCollider = true;

            bool bfIsRunMode = animator.GetBool("IsRunMode");
            animator.SetBool("IsRunMode", false);
            animator.SetFloat("MoveSpeed", 1.0f);

            transform.LookAt(checkCameraCollider.assassinateTargetPos);

            //일정 거리 이내가 될 때까지
            while (unit.assassinate.enableCollider)
            {
                rb.AddForce(transform.forward * unit.walkSpeed);
                rb.velocity *= 0.9f;
                yield return null;
            }
            animator.SetBool("IsRunMode", bfIsRunMode);
            animator.SetFloat("MoveSpeed", 0);
            animator.Play("Idle 0-0", AnimationLayers.Standing, 0);

            checkCameraCollider.InitCanAssassinate();
            checkCameraCollider.InitAssassinateTargetPos();
            unit.EnableAssassinate(true);

            unit.lockControl = false;
            yield break;
        }

        //마우스 좌측을 길게 눌러서 조준후 발사 => 소음을 발생시켜 주의분산
        private void AttackThrow()
        {
            if (unit.swManager.AttackDelayDone(curWeapon))
            {
                //조준
                if (Input.GetButton("Fire1") && pInventory.CheckWeapon(curWeapon))
                    unit.AttackPhaseAiming(throwPos.position, mainCameraTransform.rotation.eulerAngles, ref playerSpeed, ref curLookDirState);
                //조준한 상태에서 놓으면 투척
                else if ((Input.GetButtonUp("Fire1") || unit.readyToThrowItem) && unit.doubleThrowLock)
                {
                    unit.AttackPhaseThrow(throwPos.position, curWeapon, ref playerSpeed);
                    pInventory.Remove((int)curWeapon);
                }
            }
            //Throw 애니메이션이 재생 중
            else if (animator.GetBool("ThrowItem"))
            {
                unit.curUnitPose = UnitPose.MOD_THROWEND;
                SetBYCurUnitPose();
                animator.SetLayerWeight(AnimationLayers.Throw, animator.GetLayerWeight(AnimationLayers.Throw) - Time.deltaTime);
            }

            //Throw 애니메이션 종료 후
            if (animator.GetLayerWeight(AnimationLayers.Throw) <= 0 && animator.GetBool("ThrowItem"))
            {
                curLookDirState = LookDirState.IDLE;
  
                unit.ResetThrowAnimation();
                SetBYCurUnitPose();
            }
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
        private void ControlThrowMove()
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
                    destiPos = -transform.right * Input.GetAxis("Horizontal") * playerSpeed;

                    rb.AddForce(destiPos);

                    //엄폐 직후 이동이 안 되는 오류 시
                    if (rb.velocity == Vector3.zero)
                        rb.velocity = destiPos;
                }
            }

            //우측 끝에서 우측 이동 버튼을 다시 누르면 엄폐물 이동 수행
            if (animator.GetBool("IsWallRightEnd"))
            {
                if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") > 0)
                    StartCoroutine(unit.MoveEndToEnd(true));
            }
            //좌측 끝에서 좌측 이동 버튼을 다시 누르면 엄폐물 이동 수행
            else if (animator.GetBool("IsWallLeftEnd"))
            {
                if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") < 0)
                    StartCoroutine(unit.MoveEndToEnd(false));
            }
        }
        //공격에 관한 조작
        private void ControlAttack()
        {
            unit.swManager.AttackDelayManager();

            switch (curWeapon)
            {
                //비무장, 아무것도 할 수 없음
                case WeaponCode.HAND:
                    AttackDefault();
                    AttackAssassinate();
                    break;
                case WeaponCode.CAN:
                    AttackAssassinate();
                    if (unit.curUnitPose != UnitPose.MOD_CROUCH)
                        AttackThrow();
                    break;
                case WeaponCode.CHEESE:
                    AttackAssassinate();
                    if(unit.curUnitPose != UnitPose.MOD_CROUCH)
                        AttackThrow();
                    break;
                //플레이어 위치에서 사용
                case WeaponCode.SMOKE:
                    AttackAssassinate();
                    if (unit.curUnitPose != UnitPose.MOD_CROUCH)
                    {
                        if (Input.GetButtonDown("Fire1") && unit.swManager.AttackDelayDone(curWeapon) && pInventory.CheckWeapon(curWeapon))
                        {
                            unit.InstantiateWeapon(curWeapon, transform.position + ValueCollections.weaponSmokeVec, ValueCollections.weaponSmokeQuat);
                            pInventory.Remove((int)curWeapon);
                        }
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

        #region Public Methods

        //Player의 Aggro, Speed를 변경할 때 사용
        public void SetBYCurUnitPose()
        {
            switch (unit.curUnitPose)
            {
                case UnitPose.MOD_WALK:
                    aggroValue = AggroCollections.aggroWalk;
                    playerSpeed = unit.walkSpeed;
                    break;
                case UnitPose.MOD_RUN:
                    aggroValue = AggroCollections.aggroRun;
                    playerSpeed = unit.speed;
                    break;
                case UnitPose.MOD_CROUCH:
                    aggroValue = AggroCollections.aggroCrouch;
                    playerSpeed = unit.walkSpeed;
                    break;
                case UnitPose.MOD_COVERSTAND:
                    aggroValue = AggroCollections.aggroWalk;
                    playerSpeed = unit.coverSpeed;
                    break;
                case UnitPose.MOD_COVERCROUCH:
                    aggroValue = AggroCollections.aggroCrouch;
                    playerSpeed = unit.coverSpeed;
                    break;
                case UnitPose.MOD_THROW:
                    aggroValue = AggroCollections.aggroWalk;
                    playerSpeed = unit.walkSpeed;
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}

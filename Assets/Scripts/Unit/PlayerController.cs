﻿using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.GameSystem;

namespace Com.MyCompany.MyGame
{ 
    public class PlayerController : MonoBehaviour
    {
        #region Sub Classes

        private class Inventory
        {
            private UIManager uiManager;
            private bool _takeGold = false;
            private int[] _items = new int[(int)ItemCode.max - 1];
            private int[] maxAmount = new int[(int)ItemCode.max - 1];

            public bool takeGold { get { return _takeGold; } }
            public int[] items { get { return _items; } }

            public Inventory(UIManager UIManager)
            {
                uiManager = UIManager;
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = 0;
                    maxAmount[i] = ValueCollections.itemMaxAmount[i];
                }
                _takeGold = false;
            }

            public void Add(ItemCode item, int amount)
            {
                switch (item)
                {
                    case ItemCode.CAN:
                    case ItemCode.CHEESE:
                    case ItemCode.SMOKE:

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
                ShowInvnetory();
            }
            public void Add(ItemCode item)
            { 
                switch (item)
                {
                    case ItemCode.CAN:
                    case ItemCode.CHEESE:
                    case ItemCode.SMOKE:

                        int index = (int)(item) - 1;

                        if (_items[index] + 1 > maxAmount[index])
                            _items[index] = maxAmount[index];
                        else
                            _items[index]++;
                        break;
                    case ItemCode.GOLD:
                        _takeGold = true;
                        GameObject.FindWithTag("Manager").GetComponent<StageManager>().ShowEndArea();
                        break;
                    default:
                        break;
                }

                ShowInvnetory();
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

            public void Remove(ItemCode item, int amount)
            {
                switch (item)
                {
                    case ItemCode.CAN:
                    case ItemCode.CHEESE:
                    case ItemCode.SMOKE:

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

                ShowInvnetory();
            }
            public void Remove(ItemCode item)
            {
                switch (item)
                {
                    case ItemCode.CAN:
                    case ItemCode.CHEESE:
                    case ItemCode.SMOKE:

                        int index = (int)(item) - 1;

                        if (_items[index] - 1 < 0)
                            _items[index] = 0;
                        else
                            _items[index]--;
                        break;
                    case ItemCode.GOLD:
                        _takeGold = false;
                        break;
                    default:
                        break;
                }

                ShowInvnetory();
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
                uiManager.SetUILabelText("Window_Inventory", (takeGold ? "1" : "0"), "Treasure Value");
                uiManager.SetUILabelText("Window_Inventory", items[0].ToString(), "Can Value");
                uiManager.SetUILabelText("Window_Inventory", items[1].ToString(), "Cheese Value");
                uiManager.SetUILabelText("Window_Inventory", items[2].ToString(), "Smoke Value");
                //UnityEngine.Debug.Log("[ShowInventory] GOLD: " + _takeGold + ", CAN: " + _items[0] + ", CHEESE: " + _items[1] + ", SMOKE: " + _items[2]);
            }
        }

        public class NearestItem
        {
            private Transform playerTr;
            private Item _item;
            private float _dist;

            public NearestItem(Transform playerTransform)
            {
                playerTr = playerTransform;
                _dist = float.MaxValue;
            }
            //가장 가까운 Item만 주울 수 있음
            public void Set(Item item, Vector3 pos)
            {
                float d = Vector3.Distance(playerTr.position, pos);
                try
                {
                    if (_item.GetInstanceID() != item.GetInstanceID())
                    {
                        if (d < _dist)
                        {
                            _item = item;
                            _dist = d;
                        }
                    }
                    else
                        _dist = d;
                }
                catch (System.NullReferenceException)
                {
                    _item = item;
                    _dist = d;
                }
            }
            public void Delete(int itemInstanceID)
            {
                try
                {
                    if ((_item.GetInstanceID() == itemInstanceID) && (_item != null))
                        Init();
                }
                catch (System.Exception)
                { }
            }
            public ItemCode GetItemCode()
            {
                ItemCode result = ItemCode.max;
                try
                {
                    result = _item.code;
                    DestroyImmediate(_item.gameObject);
                    Init();
                }
                catch (System.Exception) { }
                return result;
            }
            public Item GetItem()
            {
                //if(_item != null) MyDebug.Log("ID: " + _item.GetInstanceID() + ", code: " + _item.code + ", Dist: " + _dist);
                return _item;
            }
            public void Init()
            {
                _item = null;
                _dist = float.MaxValue;
            }
        }

        #endregion

        #region Private Fields

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
        private MinimapCameraWork miniMapcam;
        private Inventory pInventory;

        private StageManager stageManager;
        private UIManager uiManager;

        private const float buttonDelay = 0.02f;

        #endregion

        #region Public Fields

        public Transform throwPos;
        public CheckCameraCollider checkCameraCollider;
        [HideInInspector]   public NearestItem nearestItem;

        public float aggroVal { get { return aggroValue; } }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            aggroValue = AggroCollections.aggroRun;
        }

        void Start()
        {
            nearestItem = new NearestItem(transform);
            unit = GetComponent<Unit>();
            playerAnimController = GetComponent<PlayerAnimationController>();
            GameObject m = GameObject.FindWithTag("Manager");
            stageManager = m.GetComponent<StageManager>();
            uiManager = m.GetComponent<UIManager>();
            rb = GetComponent<Rigidbody>();
            animator = unit.animator;

            mainCameraTransform = Camera.main.transform;
            miniMapcam = GameObject.Find("MinimapCamera").GetComponent<MinimapCameraWork>();
            lookDir = mainCameraTransform.forward + transform.position;

            playerSpeed = unit.speed;

            //인벤토리 초기화
            pInventory = new Inventory(uiManager);
            pInventory.Add(ItemCode.CAN, 50);
            pInventory.Add(ItemCode.CHEESE, 30);
            pInventory.Add(ItemCode.SMOKE, 20);
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
                    unit.throwLine.HideLines();
                    playerAnimController.unitAnimController.TurnOffAllLayers();
                    playerSpeed = unit.speed;
                    uiManager.ControlEquippedWeapon(curWeapon);
                    SendMessage("PlayAudio", "ChangeWeapon");
                }
                else if (Input.GetButtonDown("Weapon2"))
                {
                    curWeapon = WeaponCode.CAN;
                    uiManager.ControlEquippedWeapon(curWeapon);
                    SendMessage("PlayAudio", "ChangeWeapon");
                }
                else if (Input.GetButtonDown("Weapon3"))
                {
                    curWeapon = WeaponCode.CHEESE;
                    uiManager.ControlEquippedWeapon(curWeapon);
                    SendMessage("PlayAudio", "ChangeWeapon");
                }
                else if (Input.GetButtonDown("Weapon4"))
                {
                    curWeapon = WeaponCode.SMOKE;
                    unit.throwLine.HideLines();
                    playerSpeed = unit.speed;
                    playerAnimController.unitAnimController.TurnOffAllLayers();
                    uiManager.ControlEquippedWeapon(curWeapon);
                    SendMessage("PlayAudio", "ChangeWeapon");
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
                        case UnitPose.MOD_RUN:
                        case UnitPose.MOD_CROUCH:
                            ControlMove();
                            ControlAttack();
                            break;
                        case UnitPose.MOD_COVERSTAND:
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
                ControlGetItem();
            }

            //UnityEngine.Debug.Log("Aggro: " + aggro);
            //플레이어가 의도하지 않은 회전 방지           
            rb.angularVelocity = Vector3.zero;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Floor"))
            {
                try
                {
                    miniMapcam.ChangeFloor(other.GetComponent<Floor>().floor);
                }
                catch (System.Exception){ }
            }

            if (other.CompareTag("EndArea"))
            {
                Time.timeScale = 0;
                GameObject.FindWithTag("Manager").GetComponent<StageManager>().GameClear();
            }
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
            switch (unit.curLookDir)
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
            if (Input.GetButtonDown("Fire1"))
            {
                playerSpeed = unit.speed;
                if (checkCameraCollider.assassinateTargetPos != ValueCollections.initPos)
                    transform.LookAt(checkCameraCollider.assassinateTargetPos);
                unit.AttackDefault(true);
                unit.curLookDir = LookDirState.max;
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
                    if (unit.swManager.SWDelayDone(WeaponCode.HAND))
                    {
                        if (Input.GetButton("Assassinate") && checkCameraCollider.canAssassinate)
                        {
                            if(uiManager.IsFullUIBasicSprite("AssassinateIndicator"))
                                StartCoroutine(AssassinateMove());
                            else
                                uiManager.FillAmountUIName("AssassinateIndicator", buttonDelay);
                        }
                        else
                            uiManager.SetFillAmountUIName("AssassinateIndicator", 0);
                    }
                    else
                        checkCameraCollider.InitAssassinateTargetPos();

                    break;
                default:
                    break;
            }
        }
        private IEnumerator AssassinateMove()
        {
            try
            {
                checkCameraCollider.assassinateTarget.GetComponent<EnemyController>().assassinateTargetted = true;
            }
            catch (System.NullReferenceException) { yield break; }
            transform.LookAt(checkCameraCollider.assassinateTargetPos);

            SendMessage("EnableAudio", false);
            unit.curUnitPose = UnitPose.MOD_ATTACK;
            unit.lockControl = true;
            unit.assassinate.enableCollider = true;
            unit.EnableAssassinate(true);

            SendMessage("OffIndicator", "AssassinateIndicator");

            //일정 거리 이내가 될 때까지
            while (unit.assassinate.enableCollider)
            {
                rb.AddForce(transform.forward * unit.walkSpeed);
                yield return null;
            }
            SendMessage("EnableAudio", true);
            animator.SetBool("ReadyAssassinateAnim", true);

            animator.Play("Idle 0-0", AnimationLayers.Standing, 0);

            checkCameraCollider.InitCanAssassinate();
            checkCameraCollider.InitAssassinateTargetPos();

            unit.lockControl = false;
            yield break;
        }

        //마우스 좌측을 길게 눌러서 조준후 발사 => 소음을 발생시켜 주의분산
        private void AttackThrow()
        {
            if (unit.swManager.SWDelayDone(curWeapon))
            {
                //조준
                if (Input.GetButton("Fire1") && pInventory.CheckWeapon(curWeapon))
                    unit.AttackPhaseAiming(throwPos.position, mainCameraTransform.rotation.eulerAngles, ref playerSpeed);
                //조준한 상태에서 놓으면 투척
                else if ((Input.GetButtonUp("Fire1") || unit.readyToThrowItem) && unit.doubleThrowLock)
                {
                    unit.AttackPhaseThrow(throwPos.position, curWeapon, ref playerSpeed);
                    pInventory.Remove((int)curWeapon);
                    animator.SetBool("ThrowItemCode", true);
                }
            }
            //Throw 애니메이션이 재생 중
            else if (animator.GetBool("ThrowItemCode"))
            {
                unit.curUnitPose = UnitPose.MOD_THROWEND;
                SetBYCurUnitPose();
                animator.SetLayerWeight(AnimationLayers.Throw, animator.GetLayerWeight(AnimationLayers.Throw) - Time.deltaTime);
            }

            //Throw 애니메이션 종료 후
            if (animator.GetLayerWeight(AnimationLayers.Throw) <= 0 && animator.GetBool("ThrowItemCode"))
            {
                unit.curLookDir = LookDirState.IDLE;
  
                unit.ResetThrowAnimation();
                SetBYCurUnitPose();
                animator.SetBool("ThrowItemCode", false);
            }
        }

        //일반적인 움직임 조작
        private void ControlMove()
        {
            unit.curLookDir = LookDirState.IDLE;

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
                    destiPos = transform.up * unit.jumpPower;

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

            unit.curLookDir = LookDirState.COVER;

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
            unit.swManager.SWDelayManager();

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
                        if (Input.GetButtonDown("Fire1") && unit.swManager.SWDelayDone(curWeapon) && pInventory.CheckWeapon(curWeapon))
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
            if (Input.GetButton("GetItem") && (nearestItem.GetItem() != null))
            {
                if (uiManager.IsFullUIBasicSprite("NearestItemIndicator"))
                {
                    pInventory.Add(nearestItem.GetItemCode());
                    nearestItem.Init();
                    SendMessage("PlayAudio", "GetItem");
                    uiManager.SetIndicator("NearestItemIndicator", null);
                }
                else
                    uiManager.FillAmountUIName("NearestItemIndicator", buttonDelay);
            }
            else
                uiManager.SetFillAmountUIName("NearestItemIndicator", 0);
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

        public void SetNeareastItem(Collider other, bool onoff)
        {
            try
            {
                if (unit.health > 0)
                {
                    if (!onoff)
                    {
                        nearestItem.Init();
                        uiManager.SetIndicator("NearestItemIndicator", null);
                    }
                    else
                    {
                        nearestItem.Set(other.GetComponent<Item>(), other.transform.position);
                        uiManager.SetIndicator("NearestItemIndicator", nearestItem.GetItem().transform);
                    }
                }
            }
            catch (System.Exception) { }
        }

        public void SetIndicator(string name, Transform tr)
        {
            uiManager.SetIndicator(name, tr);
        }

        public void ShowDeadWindow()
        {
            transform.Find("PlayerRadar").GetComponent<PlayerRadar>().enabled = false;
            uiManager.ShowResultWindow(false);
        }

        #endregion
    }
}

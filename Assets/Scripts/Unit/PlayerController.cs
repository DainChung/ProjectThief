using System.Collections;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.GameSystem;
using Com.MyCompany.MyGame.UI;

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
                ShowInvnetory((WeaponCode)item);
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

                ShowInvnetory((WeaponCode)item);
            }

            public void Remove(int index)
            {
                index--;

                if (_items[index] - 1 < 0)
                    _items[index] = 0;
                else
                    _items[index]--;

                ShowInvnetory((WeaponCode)(index + 1));
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

                return result;
            }
            public void ShowInvnetory(WeaponCode weapon)
            {
                UIController inventory = uiManager.GetUIController("Window_Inventory");
                inventory.SetText((takeGold ? "1" : "0"), "Treasure Value");
                inventory.SetText(items[0].ToString(), "Can Value");
                inventory.SetText(items[1].ToString(), "Cheese Value");
                inventory.SetText(items[2].ToString(), "Smoke Value");

                if (CheckWeapon(weapon))
                {
                    uiManager.SetColorUIName("Window_EquippedWeapon", Color.white, weapon);
                }
                else
                {
                    uiManager.SetFillAmountUIName("Window_EquippedWeapon", 1, weapon);
                    uiManager.SetColorUIName("Window_EquippedWeapon", Color.red, weapon);
                }
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
            /// <summary>
            /// Item을 즉시 삭제함
            /// </summary>
            /// <returns></returns>
            public ItemCode GetItemCode()
            {
                ItemCode result = ItemCode.max;
                try
                {
                    result = _item.code;
                    switch (result)
                    {
                        case ItemCode.GOLD:
                            Destroy(_item.gameObject);
                            break;
                        case ItemCode.CAN:
                        case ItemCode.CHEESE:
                            _item.gameObject.SetActive(false);
                            break;
                        default:
                            break;
                    }
                    Init();

                }
                catch (System.Exception e){ MyDebug.Log(this.ToString() + " : " + e); }
                return result;
            }
            public Item GetItem()
            {
                //if(_item != null) MyDebug.Log("ID: " + _item.GetInstanceID() + ", code: " + _item.code + ", Dist: " + _dist);
                return _item;
            }
            public ControlledStructure GetStructure()
            {
                return _item.GetComponent<ControlledStructure>();
            }
            public void Init()
            {
                _item = null;
                _dist = float.MaxValue;
            }
        }

        #endregion

        #region Private Fields

        private float playerSpeed;
        private Transform mainCameraTransform;
        private Quaternion destiRotation;
        private Vector3 lookDir;
        private Vector3 destiPos;
        private CharacterController charController;
        private Vector3 moveDir = Vector3.zero;

        private float mass = 1f;
        private Vector3 gravity = new Vector3(0, -9.8f, 0);

        private Animator animator;
        private PlayerAnimationController playerAnimController;

        private WeaponCode curWeapon = WeaponCode.HAND;
        private Vector3 throwRotEuler = Vector3.zero;

        //높을수록 적캐릭터에게 쉽게 들킨다.
        private float aggroValue;

        private Unit unit;
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
            animator = unit.animator;

            mainCameraTransform = Camera.main.transform;
            miniMapcam = GameObject.Find("MinimapCamera").GetComponent<MinimapCameraWork>();
            charController = GetComponent<CharacterController>();
            lookDir = mainCameraTransform.forward + transform.position;

            playerSpeed = unit.speed;

            //인벤토리 초기화
            pInventory = new Inventory(uiManager);
            pInventory.Add(ItemCode.CAN, 5);
            pInventory.Add(ItemCode.CHEESE, 3);
            pInventory.Add(ItemCode.SMOKE, 2);
        }

        void FixedUpdate()
        {
            if (!unit.IsOnFloor()) moveDir += gravity * mass * Time.deltaTime;
            else moveDir.Set(moveDir.x, 0f, moveDir.z);
            //움직임이 허용된 상태에서만 조작 가능
            if (!unit.lockControl)
            {
                ChangeWeapon();
                //바닥 위에 있을 때
                if (unit.IsOnFloor())
                {
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
                            break;
                        case UnitPose.MOD_THROWEND:
                        case UnitPose.MOD_ATTACK:
                            ControlAttack();
                            break;
                        default:
                            break;
                    }
                    SetBYCurUnitPose();
                }

                LookDir();
                ControlGetItem();
            }
            charController.Move(moveDir * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Floor"))
            {
                try { miniMapcam.ChangeFloor(other.GetComponent<Floor>().floor); }
                catch (System.Exception) { }
            }

            if (other.CompareTag("EndArea"))
            {
                Time.timeScale = 0;
                GameObject.FindWithTag("Manager").GetComponent<StageManager>().GameClear();
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Floor"))
            {
                if (other.name.Contains("Stairs")) mass = 10f;
                else mass = 1f;
            }
        }

        #endregion

        #region Private Methods

        void SetLookDir(float vertical, float horizontal)
        {
            lookDir = (vertical * mainCameraTransform.forward) + (horizontal * mainCameraTransform.right) + transform.position;
            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
        }
        void LookDir()
        {
            if (animator.GetBool("IsCovering")) unit.curLookDir = LookDirState.COVER;
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
            //플레이어가 낙하할 때 x축 또는 z축이 회전하는 현상 방지
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        private void ChangeWeapon()
        {
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

            if (pInventory.CheckWeapon(curWeapon))
                uiManager.SetFillAmountUIName("Window_EquippedWeapon", unit.swManager.GetTime(curWeapon), curWeapon);
        }
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
                    if (unit.curUnitPose != UnitPose.MOD_CROUCH)
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
                            uiManager.ControlEquippedWeaponCoolTime(curWeapon);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void AttackDefault()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                playerSpeed = unit.speed;
                if (checkCameraCollider.assassinateTargetPos != ValueCollections.initPos)
                    transform.LookAt(checkCameraCollider.assassinateTargetPos);
                unit.AttackDefault(true);
                unit.curLookDir = LookDirState.max;
                uiManager.ControlEquippedWeaponCoolTime(curWeapon);
            }
        }
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
                                uiManager.FillAmountUIName("AssassinateIndicator", buttonDelay * 2);
                        }
                        else
                            uiManager.SetFillAmountUIName("AssassinateIndicator", 0);
                    }
                    else
                        checkCameraCollider.InitAssassinateTargetPos();

                    break;
                default:
                    if (checkCameraCollider.assassinateEnemy == null) unit.curUnitState = UnitState.IDLE;
                    break;
            }
        }
        private IEnumerator AssassinateMove()
        {
            try
            {
                checkCameraCollider.assassinateEnemy.assassinateTargetted = true;
            }
            catch (System.NullReferenceException) { yield break; }
            transform.LookAt(checkCameraCollider.assassinateTargetPos);

            SendMessage("EnableAudio", false);
            unit.curUnitPose = UnitPose.MOD_ATTACK;
            unit.unitAnimController.SetCurrentAnimLayer(AnimationLayers.Assassinate);
            unit.lockControl = true;
            unit.attackCollider.InitAttackCollider(-1);
            unit.EnableAttackCollider(true);

            SendMessage("OffIndicator", "AssassinateIndicator");

            //일정 거리 이내가 될 때까지
            while (unit.attackCollider.enableCollider)
            {
                charController.Move(transform.forward * unit.walkSpeed * Time.deltaTime * 0.01f);
                yield return null;
            }
            SendMessage("EnableAudio", true);
            animator.SetBool("ReadyAssassinateAnim", true);

            animator.Play("Idle 0-0", AnimationLayers.Standing, 0);

            checkCameraCollider.InitCanAssassinate();
            checkCameraCollider.InitAssassinateTargetPos();

            unit.curUnitPose = UnitPose.MOD_RUN;
            SetBYCurUnitPose();
            unit.unitAnimController.SetCurrentAnimLayer();
            unit.lockControl = false;
            yield break;
        }
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
                    uiManager.ControlEquippedWeaponCoolTime(curWeapon);
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

        private void ControlMove()
        {
            unit.curLookDir = LookDirState.IDLE;

            //플레이어 캐릭터 이동
            if (Input.GetButton("Vertical") || Input.GetButton("Horizontal")) moveDir = transform.forward * playerSpeed * Time.deltaTime;
            else moveDir.Set(0, moveDir.y, 0);

            //점프할 때
            if (animator.GetBool("IsRunMode"))
                if (Input.GetButtonDown("Jump")) moveDir += transform.up * unit.jumpPower * Time.deltaTime;
        }
        private void ControlThrowMove()
        {
            //플레이어 캐릭터 이동
            if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
                moveDir = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * playerSpeed * Time.deltaTime;
            else moveDir.Set(0, moveDir.y, 0);
        }
        private void ControlCover()
        {
            unit.curLookDir = LookDirState.COVER;

            if (Input.GetButton("Horizontal"))
                moveDir = -transform.right * Input.GetAxis("Horizontal") * playerSpeed * Time.deltaTime;
            else
                moveDir.Set(0, 0, 0);
        }

        private void ControlGetItem()
        {
            if (Input.GetButton("GetItem") && (nearestItem.GetItem() != null))
            {
                if (uiManager.IsFullUIBasicSprite("NearestItemIndicator"))
                {
                    SetIndicator("NearestItemIndicator", null);
                    if (nearestItem.GetItem().code == ItemCode.GOLD)
                        SetIndicator("DestiIndicator", null);

                    if (nearestItem.GetItem().code == ItemCode.STRUCTURE)
                        nearestItem.GetStructure().UseStructure();
                    else
                        pInventory.Add(nearestItem.GetItemCode());

                    nearestItem.Init();
                    SendMessage("PlayAudio", "GetItem");
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
                        SetIndicator("NearestItemIndicator", null);
                    }
                    else
                    {
                        nearestItem.Set(other.GetComponent<Item>(), other.transform.position);
                        SetIndicator("NearestItemIndicator", nearestItem.GetItem().transform);
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
            uiManager.ShowResultWindow(false, false);
        }

        #endregion
    }
}

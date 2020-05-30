using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.Exceptions;

namespace Com.MyCompany.MyGame
{
    public class EnemyController : MonoBehaviour
    {
        #region Sub Struct

        struct DetectedTarget
        {
            public WeaponCode code;
            public Transform tr;
            public Vector3 pos;
        }

        #endregion

        #region Sub Classes

        //우선순위 : 치즈 > 캔 = 연막
        private class DetectedTargetQueue
        {
            public bool FindCheese { get { return (queue[0].code == WeaponCode.CHEESE); } }
            public bool FindPlayer { get { return (queue[0].code == WeaponCode.PLAYER) || (queue[0].code == WeaponCode.PLAYERTRACK); } }

            //어그로 끈 Weapon을 저장하는 배열, 최대 3개까지만 기억
            private DetectedTarget[] queue = new DetectedTarget[3];

            public DetectedTargetQueue()
            {
                DetectedTarget init;
                init.code = WeaponCode.max;
                init.tr = null;
                init.pos = ValueCollections.initPos;

                queue[0] = init;
                queue[1] = init;
                queue[2] = init;
            }

            public void Enqueue(WeaponCode weaponCode, Transform tr, Vector3 pos)
            {
                DetectedTarget input;
                input.code = weaponCode;
                input.tr = tr;
                input.pos = pos;
                //중복된 내용 아님
                if (!IsItDuplicate(input))
                {
                    if (Count() <= 2)
                    {
                        switch (weaponCode)
                        {
                            case WeaponCode.CAN:
                            case WeaponCode.SMOKE:
                            case WeaponCode.ENEMYDEAD:
                                queue[Count()] = input;
                                break;
                            //case WeaponCode.PLAYERTRACK:
                            //case WeaponCode.PLAYER:
                            //case WeaponCode.CHEESE:
                            //    Clear();
                            //    queue[0] = input;
                            //    break;
                            default:
                                break;
                        }
                    }
                    //우선 순위 CHEESE > PLAYER > PLAYERTRACK > 그 외
                    switch (weaponCode)
                    {
                        case WeaponCode.PLAYERTRACK:
                        case WeaponCode.PLAYER:
                            if (queue[0].code != WeaponCode.CHEESE)
                            {
                                Clear();
                                queue[0] = input;
                            }
                                break;
                        case WeaponCode.CHEESE:
                            Clear();
                            queue[0] = input;
                            break;
                        default:
                            break;
                    }

                    //DebugQueue();
                }
            }

            //queue[0]부터 차례대로 리턴
            public DetectedTarget Dequeue()
            {
                DetectedTarget result;

                if (Count() > 0)
                {
                    result = queue[0];
                    PushQueue();
                }
                else
                {
                    result.code = WeaponCode.max;
                    result.tr = null;
                    result.pos = ValueCollections.initPos;
                }
                return result;
            }

            public void DebugQueue()
            {
                UnityEngine.Debug.Log("===================Enemy Queue==================");
                UnityEngine.Debug.Log("0) Code: " + queue[0].code + ", " + queue[0].pos);
                UnityEngine.Debug.Log("1) Code: " + queue[1].code + ", " + queue[1].pos);
                UnityEngine.Debug.Log("2) Code: " + queue[2].code + ", " + queue[2].pos);
                UnityEngine.Debug.Log("================================================");
            }

            public int Count()
            {
                int result = 0;

                for (int i = 0; i < queue.Length; i++)
                {
                    if (queue[i].code != WeaponCode.max)
                        result = i+1;
                }

                return result;
            }
            public void Clear()
            {
                queue[0].code = WeaponCode.max;
                queue[0].tr = null;
                queue[0].pos = ValueCollections.initPos;
                queue[1].code = WeaponCode.max;
                queue[1].tr = null;
                queue[1].pos = ValueCollections.initPos;
                queue[2].code = WeaponCode.max;
                queue[2].tr = null;
                queue[2].pos = ValueCollections.initPos;
            }

            //중복된 내용이면 true, 아니면 false
            private bool IsItDuplicate(DetectedTarget input)
            {
                for (int i = 0; i < queue.Length; i++)
                {
                    if ((queue[i].code == input.code) && (queue[i].pos == input.pos))
                        return true;
                }

                return false;
            }

            private void PushQueue()
            {
                for (int i = 1; i < queue.Length; i++)
                {
                    queue[i - 1] = queue[i];
                    queue[i].code = WeaponCode.max;
                    queue[i].tr = null;
                    queue[i].pos = ValueCollections.initPos;
                }
            }
            private void PushQueueReverse()
            {
                for (int i = queue.Length - 2; i >= 0; i--)
                {
                    queue[i + 1] = queue[i];
                }

                queue[0].code = WeaponCode.max;
                queue[0].tr = null;
                queue[0].pos = ValueCollections.initPos;
            }

        }

        public class TargetManager
        {
            private List<GameObject> targets = new List<GameObject>();

            public void Add(GameObject item)
            {
                targets.Add(item);
            }
            public void Clear()
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if(targets[i] != null)
                        Destroy(targets[i]);
                }
                targets.Clear();
            }
        }

        #endregion

        #region Private Fields

        private DetectedTargetQueue queue = new DetectedTargetQueue();

        private Unit unit;
        private Rigidbody rb;

        private DetectedTarget curTarget;
        private bool _doesReachToTarget = false;

        private Stopwatch stayDelay = new Stopwatch();
        private bool checkStayDelay { get { return stayDelay.ElapsedMilliseconds >= ValueCollections.enemyDetectedStayMax[(int)curTarget.code]; } }

        private NavMeshAgent agent;
        private float enemySpeed;
        private bool isMovingNow = false;
        private bool canIAttack = false;
        private bool _canAssassinate = false;

        private Transform player = null;
        private Vector3 playerPosition { get { return (player == null) ? ValueCollections.initPos : new Vector3(player.position.x, transform.position.y, player.position.z); } }

        private Unit.StopwatchManager behaveSWManager;
        private int patrolSpotIndex = 0;

        private SpriteRenderer[] icons = new SpriteRenderer[2];

        #endregion

        #region Public Fields

        public Transform rightArm;

        public Transform throwPos;
        public float moveSpeed { get { return isMovingNow ? 1 : 0; } }
        public bool doesReachToTarget { set { _doesReachToTarget = value; } }
        public bool IsMovingNow { set { isMovingNow = value; } }
        public bool CanIAttack { set { canIAttack = value; } }

        [HideInInspector] public TargetManager targetMng = new TargetManager();
        [HideInInspector] public bool assassinateTargetted = false;
        [HideInInspector] public Vector3 lookDir = new Vector3();
        public MeshCollider enemyRadar;
        public MeshCollider enemyEye;
        //assassinateTargetted가 true면 무조건 false 반환
        public bool canAssassinate { get { return (assassinateTargetted ? false : _canAssassinate); } set { _canAssassinate = value; } }

        public EnemyCheckStructure checkStructure;
        public Transform[] patrolSpots = new Transform[6];

        public WeaponCode curTargetCode { get { return curTarget.code; } }
        public Vector3 curTargetPos { get { return curTarget.pos; } }

        #endregion


        #region MonoBehaviour Callbacks

        void Awake()
        {
            stayDelay.Start();
            stayDelay.Stop();
        }

        void Start()
        {
            unit = GetComponent<Unit>();
            unit.curUnitState = UnitState.IDLE;
            unit.curLookDir = LookDirState.IDLE;
            enemySpeed = unit.speed;
            long[] delayTimes = { 200, 0 };
            behaveSWManager = new Unit.StopwatchManager(delayTimes, 1);

            rb = GetComponent<Rigidbody>();

            agent = GetComponent<NavMeshAgent>();
            agent.speed = 0;
            checkStructure.enabled = true;

            icons[0] = transform.Find("ALERTIcon").GetComponent<SpriteRenderer>();
            icons[1] = transform.Find("COMBATIcon").GetComponent<SpriteRenderer>();
            _InitCurTarget();
        }
        
        void Update()
        {
            if (!unit.lockControl)
            {
                //if (unit.curUnitState == UnitState.COMBAT)
                //{
                //    isMovingNow = false;
                //    unit.AttackDefault(false);
                //}
                //else
                //    isMovingNow = true;
                if (unit.health > 0)
                    AI();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            #region Target 위치에 도달
            if (other.gameObject.layer == PhysicsLayers.TargetLayer)
            {
                try
                {
                    Target t = other.transform.GetComponent<Target>();

                    if (t.code == WeaponCode.CHEESE)
                    {
                        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 7f, transform.forward, 0.01f, 1 << PhysicsLayers.Item);

                        foreach (RaycastHit hit in hits)
                        {
                            Transform hitTR = hit.transform;

                            if (hitTR.name.Contains("CHEESE"))
                            {
                                unit.curUnitState = UnitState.EAT;
                                unit.curLookDir = LookDirState.DIRECT;
                                lookDir = hitTR.position;
                                _InitCurTarget();
                                curTarget.tr = hitTR;
                                curTarget.pos = hitTR.position;

                                enemyEye.enabled = false;
                            }
                        }
                    }
                    if (transform.GetInstanceID() == t.ID && t.code != WeaponCode.PLAYER)
                    {
                        _doesReachToTarget = true;
                        Destroy(other.transform.gameObject);
                    }
                }
                catch (System.Exception) { }
            }
            #endregion
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.TargetLayer)
            {
                try
                {
                    if (transform.GetInstanceID() == other.transform.GetComponent<Target>().ID)
                        _doesReachToTarget = false;
                }
                catch (System.Exception) { }
            }
        }

        #endregion

        #region Private Methods

        private void LookDir()
        {
            switch (unit.curLookDir)
            {
                case LookDirState.FINDPLAYER:
                    try
                    {
                        player = GameObject.FindWithTag("Player").transform;
                    }
                    catch (System.NullReferenceException)
                    {
                        unit.curUnitState = UnitState.IDLE;
                        EnemyAlertManager();
                        break;
                    }
                    transform.LookAt(playerPosition);
                    break;
                case LookDirState.AGENT:
                    break;
                case LookDirState.DIRECT:
                    lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
                    transform.LookAt(lookDir);
                    break;
                default:
                    if (curTarget.pos != ValueCollections.initPos)
                        transform.LookAt(new Vector3(curTarget.pos.x, transform.position.y, curTarget.pos.z));
                    if (player != null)
                        unit.curLookDir = LookDirState.FINDPLAYER;
                    break;
            }

            //플레이어가 낙하할 때 x축 또는 z축이 회전하는 현상 방지
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        private void AI()
        {
            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    if (!enemyRadar.enabled) enemyRadar.enabled = true;
                    Patrol();
                    break;
                case UnitState.ALERT:
                    if (!enemyRadar.enabled) enemyRadar.enabled = true;

                    if (queue.Count() == 0) Alert();
                    else ChaseTargetBYQueue();
                    break;
                case UnitState.COMBAT:
                    if (!enemyRadar.enabled) enemyRadar.enabled = true;
                    Combat();
                    break;
                case UnitState.CHEESE:
                    ChaseTargetBYQueue();
                    break;
                case UnitState.INSMOKE:
                    ChaseTargetBYQueue();
                    if (curTarget.code == WeaponCode.max)
                    {
                        enemyRadar.enabled = true;
                        unit.curUnitState = UnitState.ALERT;
                    }
                    break;
                case UnitState.EAT:
                    LookDir();
                    Move(1.2f);
                    break;
                default:
                    break;
            }
            //EnemyAlertManager();
        }

        //curUnitState == UnitState.IDLE
        private void Patrol()
        {
            if (unit.animator.GetBool("IsRunMode")) unit.animator.SetBool("IsRunMode", false);
            if (curTarget.code == WeaponCode.max)
            {
                if(patrolSpotIndex >= patrolSpots.Length) patrolSpotIndex = 0;
                try { SetCurTarget(patrolSpots[patrolSpotIndex], WeaponCode.PATROL); }
                catch (System.Exception) { }
            }
            else if (unit.curLookDir == LookDirState.FINDPLAYER)
            {
                LookDir();
                Stop();
            }
            else
                Move(1.6f);
        }

        //curUnitState == UnitState.ALERT
        private void Alert()
        {
            if (curTarget.code == WeaponCode.max)
            {
                SetCurTarget(WeaponCode.PATROL, transform.position + ValueCollections.GetRandomVector3(15.0f, GetInstanceID()));
                patrolSpotIndex++;

                if (patrolSpotIndex > 10)
                {
                    patrolSpotIndex = 0;
                    unit.SetAlertValue(0);
                    EnemyAlertManager();
                }
            }
            else
                Move(1.6f);
        }

        //curUnitState == UnitState.COMBAT
        private void Combat()
        {
            switch (curTarget.code)
            {
                case WeaponCode.CHEESE:
                    unit.curUnitState = UnitState.CHEESE;
                    StopWithoutInitCurtarget();
                    break;
                default:
                    checkStructure.CheckStructure(playerPosition);
                    ChaseTargetBYQueue();
                    break;
            }
        }

        //특정 타겟(CAN, CHEESE, Player) 위치로 이동
        //우선순위: CHEESE > Player > CAN > Patrol Spot
        private void ChaseTargetBYQueue()
        {
            LookDir();

            if (queue.Count() > 0)
            {
                if(curTarget.code == WeaponCode.PATROL && unit.curUnitState != UnitState.IDLE) _InitCurTarget();
                //queue에 뭔가 있을 때
                //현재 추적하고 있는 타겟이 없을 때만 queue에서 정보를 받아온다.
                if (curTarget.code == WeaponCode.max) SetCurTarget();
            }

            switch (curTarget.code)
            {
                case WeaponCode.CAN:
                case WeaponCode.SMOKE:
                    if (queue.FindPlayer || queue.FindCheese)
                        SetCurTarget();
                    else
                        Move(1f);
                    break;
                case WeaponCode.PLAYERTRACK:
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        Move(1.4f);
                    break;
                case WeaponCode.PLAYER:
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        CombatMove();
                    break;
                case WeaponCode.CHEESE:
                    targetMng.Clear();
                    player = null;
                    Move(1.4f);
                    if (curTargetCode == WeaponCode.max) unit.SetAlertValue(AggroCollections.alertMin);
                    break;
                default:
                    break;
            }
        }

        private void CombatMove()
        {
            if (!_doesReachToTarget)
            {
                isMovingNow = true;
                if (checkStructure.isThereStructure)
                {
                    if (!agent.enabled) agent.enabled = true;
                    unit.curLookDir = LookDirState.AGENT;
                    agent.SetDestination(playerPosition);
                }
                else
                {
                    if (agent.enabled)
                    {
                        agent.ResetPath();
                        agent.enabled = false;
                    }
                    unit.curLookDir = LookDirState.FINDPLAYER;
                    rb.velocity = transform.forward * enemySpeed * 0.15f;
                }
            }
            else
            {
                unit.AttackDefault(false);
                isMovingNow = false;
            }
            rb.velocity *= 0.9f;
        }
        private void Move(float dist)
        {
            try
            {
                ValidateException.CheckAIsCloseToB(transform.position, curTarget.pos, dist);
                if (assassinateTargetted) throw new AIsCloseToB();
                checkStructure.CheckStructure(curTarget.pos);
                if (!_doesReachToTarget)
                {
                    isMovingNow = true;
                    if (checkStructure.isThereStructure)
                    {
                        if (!agent.enabled) agent.enabled = true;

                        unit.curLookDir = LookDirState.AGENT;
                        agent.SetDestination(curTarget.pos);
                    }
                    else
                    {
                        if (agent.enabled)
                        {
                            agent.ResetPath();
                            agent.enabled = false;
                        }
                        unit.curLookDir = LookDirState.IDLE;
                        LookDir();
                        rb.velocity = transform.forward * enemySpeed * 0.15f;
                    }
                }
                else if (_doesReachToTarget)
                {
                    if (!stayDelay.IsRunning) stayDelay.Restart();

                    if (checkStayDelay)
                    {
                        Stop();
                        stayDelay.Stop();
                    }
                }
                rb.velocity *= 0.9f;
            }
            catch (AIsCloseToB)
            {
                Stop();
                if(unit.curUnitState == UnitState.INSMOKE) enemyRadar.enabled = true;
            }
        }

        private void SetCurTarget()
        {
            curTarget = queue.Dequeue();
            GameObject targetOBJ = Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);

            unit.curLookDir = LookDirState.IDLE;
            if (player != null)
            {
                targetOBJ.transform.parent = player;
                targetMng.Add(targetOBJ);
                unit.curLookDir = LookDirState.FINDPLAYER;

                if (stayDelay.ElapsedMilliseconds > 100)
                {
                    GameObject obj = Instantiate(Resources.Load(string.Format("{0}Aggro", FilePaths.weaponPath)) as GameObject, transform.position, transform.rotation) as GameObject;
                    obj.GetComponent<Aggro>().SetCode(WeaponCode.PLAYER, 6f);
                }
            }
            stayDelay.Reset();
            stayDelay.Stop();
            targetOBJ.GetComponent<Target>().SetTarget(transform.GetInstanceID(), curTarget.code, this);
            if (curTargetCode != WeaponCode.PLAYER || curTargetCode != WeaponCode.PLAYERTRACK) player = null;
        }
        private void SetCurTarget(WeaponCode code, Vector3 pos)
        {
            curTarget.code = code;
            curTarget.pos = pos;
            GameObject targetOBJ = Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);

            if (player != null)
            {
                targetOBJ.transform.parent = player;
                targetMng.Add(targetOBJ);

                if (code != WeaponCode.PLAYER) player = null;
                if (stayDelay.ElapsedMilliseconds > 100)
                {
                    GameObject obj = Instantiate(Resources.Load(string.Format("{0}Aggro", FilePaths.weaponPath)) as GameObject, transform.position, transform.rotation) as GameObject;
                    obj.GetComponent<Aggro>().SetCode(WeaponCode.PLAYER, 6f);
                }
            }
            stayDelay.Reset();
            stayDelay.Stop();
            targetOBJ.GetComponent<Target>().SetTarget(transform.GetInstanceID(), curTarget.code, this);
        }
        private void SetCurTarget(Transform tr, WeaponCode code)
        {
            curTarget.tr = tr;
            SetCurTarget(code, tr.position);
        }
        private void _InitCurTarget()
        {
            if (curTarget.code != WeaponCode.max && curTarget.pos != ValueCollections.initPos)
            {
                curTarget.code = WeaponCode.max;
                curTarget.tr = null;
                curTarget.pos = ValueCollections.initPos;
            }
            if (agent.enabled) agent.ResetPath();
            isMovingNow = false;
            stayDelay.Reset();
            stayDelay.Stop();
        }
        private void StopWithoutInitCurtarget()
        {
            _doesReachToTarget = false;
            isMovingNow = false;
            canIAttack = true;
        }

        private void ShowIcon()
        {
            if (unit.curUnitState == UnitState.ALERT)
            {
                icons[0].enabled = true;
                icons[1].enabled = false;
            }

            switch (unit.curUnitState)
            {
                case UnitState.ALERT:
                    icons[0].enabled = true;
                    icons[1].enabled = false;
                    break;
                case UnitState.COMBAT:
                    icons[0].enabled = false;
                    icons[1].enabled = true;
                    break;
                default:
                    icons[0].enabled = false;
                    icons[1].enabled = false;
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void InitCurTarget()
        {
            player = null;
            _InitCurTarget();
        }

        public void Stop()
        {
            switch (unit.curUnitState)
            {
                case UnitState.EAT:
                    if (curTarget.tr != null)
                    {
                        DisposeCheese();
                        //줍는 애니메이션 수행
                        unit.animator.SetInteger("DeadAnim", -2);
                        unit.animator.Play("Eat Cheese", AnimationLayers.Standing);
                        unit.lockControl = true;
                    }
                    break;
                case UnitState.COMBAT:
                    if(!checkStructure.isThereStructure)
                        unit.curLookDir = LookDirState.FINDPLAYER;
                    break;
                case UnitState.IDLE:
                    patrolSpotIndex++;
                    break;
                default:
                    break;
            }

            if (agent.enabled)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            _doesReachToTarget = false;
            isMovingNow = false;
            canIAttack = true;
            _InitCurTarget();
        }
        public void Detect(WeaponCode code, Transform tr, Vector3 pos)
        {
            if (tr != null && unit.health > 0)
            {
                switch (code)
                {
                    case WeaponCode.PLAYER:
                        if (unit.alertValue < AggroCollections.combatMin)
                            unit.alertValue = AggroCollections.combatMin;
                        queue.Enqueue(code, tr, pos);
                        break;
                    case WeaponCode.CHEESE:
                        if (unit.alertValue < AggroCollections.combatMin)
                            unit.alertValue = AggroCollections.combatMin;
                        SetCurTarget(code, pos);
                        queue.Clear();
                        break;
                    case WeaponCode.SMOKE:
                    case WeaponCode.ENEMYDEAD:
                    case WeaponCode.CAN:
                    case WeaponCode.PLAYERTRACK:
                        if (unit.alertValue < AggroCollections.alertMin)
                            unit.alertValue = AggroCollections.alertMin;
                        agent.speed = 2.5f;
                        enemySpeed = unit.walkSpeed;
                        queue.Enqueue(code, tr, pos);
                        break;
                    default:
                        break;
                }
                if(agent.enabled)
                    agent.ResetPath();

                if(code == WeaponCode.SMOKE)
                    EnemyAlertManager(UnitState.INSMOKE);
                else
                    EnemyAlertManager();
            }
        }

        public bool IsCombatState()
        {
            return (unit.curUnitState == UnitState.COMBAT);
        }

        public void EnemyAlertManager()
        {
            unit.AlertManager();

            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    agent.speed = 2.5f;
                    enemySpeed = unit.walkSpeed * 1.5f;
                    break;
                case UnitState.INSMOKE:
                    agent.speed = 2.0f;
                    enemySpeed = unit.walkSpeed * 1.3f;
                    enemyEye.enabled = false;
                    break;
                case UnitState.ALERT:
                    agent.speed = 2.0f;
                    enemySpeed = unit.walkSpeed * 1.3f;
                    if (playerPosition != ValueCollections.initPos)
                        curTarget.tr = player;
                    break;
                case UnitState.COMBAT:
                    agent.speed = 2.7f;
                    enemySpeed = unit.speed * 1.5f;
                    break;
                case UnitState.CHEESE:
                case UnitState.EAT:
                    agent.speed = 2.7f;
                    enemySpeed = unit.speed * 1.5f;
                    enemyEye.enabled = false;
                    break;
                default:
                    break;
            }

            ShowIcon();
        }
        public void EnemyAlertManager(UnitState state)
        {
            unit.AlertManager();
            unit.curUnitState = state;

            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    agent.speed = 2.5f;
                    enemySpeed = unit.walkSpeed * 1.5f;
                    break;
                case UnitState.INSMOKE:
                case UnitState.ALERT:
                    agent.speed = 2.0f;
                    enemySpeed = unit.walkSpeed * 1.3f;
                    if (playerPosition != ValueCollections.initPos)
                        curTarget.tr = player;
                    break;
                case UnitState.COMBAT:
                    agent.speed = 2.7f;
                    enemySpeed = unit.speed * 1.5f;
                    break;
                default:
                    break;
            }

            ShowIcon();
        }

        public void DisposeCheese()
        {
            curTarget.tr.GetComponent<WeaponThrow>().DisposeImmediately();
        }

        #endregion

    }
}

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
        //치즈 먹고 수면 상태 빠지면 Queue를 비워야?
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
        private Vector3 playerPosition { get { return (player == null) ? ValueCollections.initPos : player.position; } }

        private Unit.StopwatchManager behaveSWManager;

        #endregion

        #region Public Fields

        public Transform throwPos;
        public float moveSpeed { get { return isMovingNow ? 1 : 0; } }
        public bool doesReachToTarget { set { _doesReachToTarget = value; } }
        public bool IsMovingNow { set { isMovingNow = value; } }
        public bool CanIAttack { set { canIAttack = value; } }

        [HideInInspector] public TargetManager targetMng = new TargetManager();
        [HideInInspector] public bool assassinateTargetted = false;
        //assassinateTargetted가 true면 무조건 false 반환
        public bool canAssassinate { get { return (assassinateTargetted ? false : _canAssassinate); } set { _canAssassinate = value; } }

        public EnemyCheckStructure checkStructure;

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

            unit.animator.SetBool("IsRunMode", false);

            rb = GetComponent<Rigidbody>();

            agent = GetComponent<NavMeshAgent>();
            agent.speed = 0;
            checkStructure.enabled = true;

            InitCurTarget();
        }
        
        void FixedUpdate()
        {
            if (!unit.lockControl)
            {
                if (unit.health > 0)
                    AI();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Target"))
            {
                _doesReachToTarget = true;
                if(transform.GetInstanceID() == other.transform.GetComponent<Target>().ID)
                    Destroy(other.transform.gameObject);
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Target"))
                _doesReachToTarget = false;
        }

        #endregion

        #region Private Methods

        private void LookDir()
        {
            switch (unit.curLookDir)
            {
                case LookDirState.FINDPLAYER:
                    player = GameObject.FindWithTag("Player").transform;
                    transform.LookAt(playerPosition);
                    break;
                case LookDirState.AGENT:
                    break;
                default:
                    if (curTarget.pos != ValueCollections.initPos)
                        transform.LookAt(new Vector3(curTarget.pos.x, transform.position.y, curTarget.pos.z));
                    if (player != null)
                        unit.curLookDir = LookDirState.FINDPLAYER;
                    break;
            }
        }

        private void AI()
        {
            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    ChaseTargetBYQueue();
                    break;
                case UnitState.ALERT:
                    ChaseTargetBYQueue();
                    break;
                case UnitState.COMBAT:
                    Combat();
                    break;
                case UnitState.CHEESE:
                    ChaseTargetBYQueue();
                    break;
                case UnitState.INSMOKE:
                    ChaseTargetBYQueue();
                    break;
                case UnitState.SLEEP:
                    break;
                default:
                    break;
            }
        }

        //curUnitState == UnitState.IDLE
        private void Patrol()
        {

        }

        //curUnitState == UnitState.ALERT
        private void Alert()
        {

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
                    try
                    {
                        ValidateException.CheckAIsCloseToB(transform.position, playerPosition, 1.5f);
                        checkStructure.CheckStructure(playerPosition);
                        ChaseTargetBYQueue();
                    }
                    catch (AIsCloseToB)
                    {
                        LookDir();
                        Stop();
                        unit.AttackDefault(false);
                    }
                    break;
            }
        }

        //특정 타겟(CAN, CHEESE, Player) 위치로 이동
        //우선순위: CHEESE > Player > CAN > Patrol Spot
        private void ChaseTargetBYQueue()
        {
            LookDir();

            //queue에 뭔가 있을 때
            if (queue.Count() > 0 && curTarget.code == WeaponCode.max)
            {
                //현재 추적하고 있는 타겟이 없을 때만 queue에서 정보를 받아온다.
                SetCurTarget();
            }

            switch (curTarget.code)
            {
                case WeaponCode.CAN:
                case WeaponCode.SMOKE:
                    if (queue.FindPlayer || queue.FindCheese)
                        SetCurTarget();
                    else
                        Move();
                    break;
                case WeaponCode.PLAYERTRACK:
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        Move();
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
                    Move();
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
                    unit.curLookDir = LookDirState.AGENT;
                    agent.SetDestination(playerPosition);
                }
                else
                {
                    unit.curLookDir = LookDirState.FINDPLAYER;
                    rb.velocity = transform.forward * enemySpeed * 0.15f;
                    agent.ResetPath();
                }
            }
            else if (_doesReachToTarget)
            {
                isMovingNow = false;
                _doesReachToTarget = false;
            }
            rb.velocity *= 0.9f;
        }
        private void Move()
        {
            try
            {
                checkStructure.CheckStructure(curTarget.pos);
                ValidateException.CheckAIsCloseToB(transform.position, curTarget.pos, 1.4f);
                if (!_doesReachToTarget)
                {
                    isMovingNow = true;
                    if (checkStructure.isThereStructure)
                    {
                        unit.curLookDir = LookDirState.AGENT;
                        agent.SetDestination(curTarget.pos);
                    }
                    else
                    {
                        unit.curLookDir = LookDirState.IDLE;
                        rb.velocity = transform.forward * enemySpeed * 0.15f;
                        agent.ResetPath();
                    }
                }
                else if (_doesReachToTarget)
                {
                    isMovingNow = false;
                    if (!stayDelay.IsRunning)
                        stayDelay.Restart();

                    if (checkStayDelay)
                    {
                        InitCurTarget();
                        _doesReachToTarget = false;
                    }
                }
                rb.velocity *= 0.9f;
            }
            catch (AIsCloseToB)
            {
                Stop();
            }
        }

        private void SetCurTarget()
        {
            stayDelay.Reset();
            stayDelay.Stop();
            curTarget = queue.Dequeue();
            GameObject targetOBJ = Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);
            unit.curLookDir = LookDirState.IDLE;
            if (player != null)
            {
                targetOBJ.transform.parent = player;
                targetMng.Add(targetOBJ);
                unit.curLookDir = LookDirState.FINDPLAYER;
            }
            targetOBJ.GetComponent<Target>().SetID(transform.GetInstanceID());
        }
        private void SetCurTarget(WeaponCode code, Vector3 pos)
        {
            stayDelay.Reset();
            stayDelay.Stop();
            curTarget.code = code;
            curTarget.pos = pos;
            GameObject targetOBJ = Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);
            if (player != null)
            {
                targetOBJ.transform.parent = player;
                targetMng.Add(targetOBJ);
            }
            targetOBJ.GetComponent<Target>().SetID(transform.GetInstanceID());
        }
        private void InitCurTarget()
        {
            isMovingNow = false;
            agent.ResetPath();
            curTarget.code = WeaponCode.max;
            curTarget.tr = null;
            curTarget.pos = ValueCollections.initPos;
            stayDelay.Reset();
            stayDelay.Stop();
        }
        private void StopWithoutInitCurtarget()
        {
            _doesReachToTarget = false;
            isMovingNow = false;
            canIAttack = true;
        }

        #endregion

        #region Public Methods

        public void Stop()
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            _doesReachToTarget = false;
            isMovingNow = false;
            canIAttack = true;
            InitCurTarget();
        }
        public void Detect(WeaponCode code, Transform tr, Vector3 pos)
        {
            if (tr != null)
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
                agent.ResetPath();
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
                    enemySpeed = unit.walkSpeed * 2;
                    break;
                case UnitState.ALERT:
                    agent.speed = 2.0f;
                    enemySpeed = unit.walkSpeed * 2;
                    if (playerPosition != ValueCollections.initPos)
                        curTarget.tr = player;
                    break;
                case UnitState.COMBAT:
                    agent.speed = 2.7f;
                    enemySpeed = unit.speed * 2;
                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}

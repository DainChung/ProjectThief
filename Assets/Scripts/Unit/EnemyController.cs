using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyController : MonoBehaviour
    {
        #region Sub Struct

        struct DetectedTarget
        {
            public WeaponCode code;
            public Transform tr;
            public Vector3 pos { get { return (tr == null) ? ValueCollections.initPos : tr.position; } }
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

                queue[0] = init;
                queue[1] = init;
                queue[2] = init;
            }

            public void Enqueue(WeaponCode weaponCode, Transform tr)
            {
                DetectedTarget input;
                input.code = weaponCode;
                input.tr = tr;

                //중복된 내용 아님
                if (!IsItDuplicate(input))
                {
                    if (Count() <= 2)
                    {
                        if (weaponCode != WeaponCode.CHEESE && weaponCode != WeaponCode.max)
                            queue[Count()] = input;
                    }

                    //PlayerTrack, Player, Cheese면 해당 타겟만 추적한다.
                    switch (weaponCode)
                    {
                        case WeaponCode.PLAYERTRACK:
                        case WeaponCode.PLAYER:
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
            }
            private void Clear()
            {
                queue[0].code = WeaponCode.max;
                queue[0].tr = null;
                queue[1].code = WeaponCode.max;
                queue[1].tr = null;
                queue[2].code = WeaponCode.max;
                queue[2].tr = null;
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
        private bool isMovingNow = false;

        private Transform player = null;
        private Vector3 playerPosition { get { return (player == null) ? ValueCollections.initPos : player.position; } }

        #endregion

        #region Public Fields

        public Transform throwPos;
        public float moveSpeed { get { return isMovingNow ? 1 : 0; } }
        public bool doesReachToTarget { set { _doesReachToTarget = value; } }
        public bool IsMovingNow { set { isMovingNow = value; } }

        [HideInInspector]
        public bool seenByCamera = false;

        public EnemyRadarManager radarMng;

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

            unit.animator.SetBool("IsRunMode", false);

            rb = GetComponent<Rigidbody>();
            InitCurTarget();

            agent = GetComponent<NavMeshAgent>();
            agent.speed = 0;
            radarMng.enabled = true;
        }
        
        void FixedUpdate()
        {
            if (!unit.lockControl)
            {
                if(unit.health > 0)
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
                    playerPosition.Set(playerPosition.x, transform.position.y, playerPosition.z);
                    transform.LookAt(playerPosition);
                    break;
                default:
                    if (curTarget.tr != null)
                        transform.LookAt(new Vector3(curTarget.pos.x, transform.position.y, curTarget.pos.z));
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
            ChaseTargetBYQueue();
        }

        //특정 타겟(CAN, CHEESE, Player) 위치로 이동
        //우선순위: CHEESE > Player > CAN > Patrol Spot
        private void ChaseTargetBYQueue()
        {
            //queue에 뭔가 있을 때
            if (queue.Count() > 0)
            {
                //현재 추적하고 있는 타겟이 없을 때만 queue에서 정보를 받아온다.
                if (curTarget.code == WeaponCode.max)
                    SetCurTarget();
            }
            LookDir();

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
                case WeaponCode.PLAYER:
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        Move();
                    break;
                case WeaponCode.CHEESE:
                    unit.curLookDir = LookDirState.IDLE;
                    Move();
                    break;
                default:
                    break;
            }
        }

        private void Move()
        {
            if (!_doesReachToTarget)
            {
                switch (curTarget.code)
                {
                    case WeaponCode.PLAYER:
                    case WeaponCode.PLAYERTRACK:
                    case WeaponCode.CAN:
                    case WeaponCode.CHEESE:
                        isMovingNow = true;
                        if (radarMng.thereIsStructure)
                        {
                            unit.curLookDir = LookDirState.IDLE;
                            agent.SetDestination(curTarget.pos);
                        }
                        else
                            rb.velocity = transform.forward * unit.speed * 0.1f;
                        break;
                    default:
                        break;
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
            rb.velocity *= 0.99f;
        }

        private void SetCurTarget()
        {
            curTarget = queue.Dequeue();
            GameObject targetOBJ = Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);
            if (player != null)
                targetOBJ.transform.parent = player;
            targetOBJ.GetComponent<Target>().SetID(transform.GetInstanceID());
        }

        private void InitCurTarget()
        {
            curTarget.code = WeaponCode.max;
            curTarget.tr = null;
            stayDelay.Stop();
        }

        #endregion

        #region Public Methods

        public void Detect(WeaponCode code, Transform tr)
        {
            if (tr != null)
            {
                switch (code)
                {
                    case WeaponCode.CAN:
                    case WeaponCode.SMOKE:
                    case WeaponCode.PLAYERTRACK:
                        if (unit.alertValue < AggroCollections.alertMin)
                            unit.alertValue = AggroCollections.alertMin;
                        break;
                    case WeaponCode.PLAYER:
                        if (unit.alertValue < AggroCollections.combatMin)
                            unit.alertValue = AggroCollections.combatMin;
                        break;
                    case WeaponCode.CHEESE:
                        if (unit.alertValue < AggroCollections.combatMin)
                            unit.alertValue = AggroCollections.combatMin;
                        break;
                    default:
                        break;
                }
                //MyDebug.Log("EnemySpeed: "+agent.speed);

                queue.Enqueue(code, tr);
                EnemyAlertManager();
            }
        }
        public bool IsCombatState()
        {
            return (unit.curUnitState == UnitState.COMBAT);
        }
        public bool IsAlertValueSmallerThen(float value)
        {
            return unit.alertValue < value;
        }
        public void SetAlertValue(float value)
        {
            unit.alertValue = value;
            EnemyAlertManager();
        }

        public void EnemyAlertManager()
        {
            unit.AlertManager();

            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    agent.speed = 4.0f;
                    break;
                case UnitState.ALERT:
                    agent.speed = 2.5f;
                    if (playerPosition != ValueCollections.initPos)
                        curTarget.tr = player;
                    break;
                case UnitState.COMBAT:
                    agent.speed = 9.0f;
                    break;
                default:
                    break;
            }

            StartCoroutine(radarMng.CheckToTarget(curTarget.pos));
        }

        #endregion

    }
}

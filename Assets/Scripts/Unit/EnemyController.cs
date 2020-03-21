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
            public Vector3 pos;
        }

        #endregion

        #region Sub Classes

        //우선순위 : 치즈 > 캔 = 연막
        //치즈 먹고 수면 상태 빠지면 Queue를 비워야?
        private class DetectedTargetQueue
        {
            public bool FindCheese { get { return (queue[0].code == WeaponCode.CHEESE); } }
            public bool FindPlayer { get { return (queue[0].code == WeaponCode.PLAYER); } }

            //어그로 끈 Weapon을 저장하는 배열, 최대 3개까지만 기억
            private DetectedTarget[] queue = new DetectedTarget[3];

            public DetectedTargetQueue()
            {
                DetectedTarget init;
                init.code = WeaponCode.max;
                init.pos = new Vector3(-1,-1,-1);

                queue[0] = init;
                queue[1] = init;
                queue[2] = init;
            }

            public void Enqueue(WeaponCode weaponCode, Vector3 position)
            {
                DetectedTarget input;
                input.code = weaponCode;
                input.pos = position;

                //중복된 내용 아님
                if (!IsItDuplicate(input))
                {
                    if (Count() <= 2)
                    {
                        if (weaponCode != WeaponCode.CHEESE && weaponCode != WeaponCode.max)
                            queue[Count()] = input;
                    }

                    if (weaponCode == WeaponCode.PLAYER)
                    {
                        //Player면 최우선 순위 2번째로 설정
                        PushQueueReverse();
                        queue[0] = input;
                    }

                    if (weaponCode == WeaponCode.CHEESE)
                    {
                        //치즈면 최우선순위로 설정(PLAYER보다 중요)
                        PushQueueReverse();
                        queue[0] = input;
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
                    result.pos = new Vector3(-1, -1, -1);
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
                    queue[i].pos.Set(-1,-1,-1);
                }
            }
            private void PushQueueReverse()
            {
                for (int i = queue.Length - 2; i >= 0; i--)
                {
                    queue[i + 1] = queue[i];
                }

                queue[0].code = WeaponCode.max;
                queue[0].pos.Set(-1, -1, -1);
            }
        }

        #endregion

        #region Private Fields

        private DetectedTargetQueue queue = new DetectedTargetQueue();

        private LookDirState curLookDir;

        private Unit unit;
        private Rigidbody rb;

        private DetectedTarget curTarget;
        private bool doesReachToTarget = false;

        private Stopwatch stayDelay = new Stopwatch();
        private bool checkStayDelay { get { return stayDelay.ElapsedMilliseconds >= ValueCollections.enemyDetectedStayMax[(int)curTarget.code]; } }

        private NavMeshAgent agent;

        #endregion

        #region Public Fields

        public Transform throwPos;
        public float moveSpeed { get { return (agent.velocity != Vector3.zero) ? 1.0f : 0.0f; } }

        #endregion


        #region MonoBehaviour Callbacks

        void Awake()
        {
            curLookDir = LookDirState.IDLE;

            stayDelay.Start();
            stayDelay.Stop();
        }

        void Start()
        {
            unit = GetComponent<Unit>();
            unit.curUnitState = UnitState.IDLE;

            unit.animator.SetBool("IsRunMode", false);

            rb = GetComponent<Rigidbody>();
            InitCurTarget();

            agent = GetComponent<NavMeshAgent>();
        }
        

        void FixedUpdate()
        {
            ChaseTarget();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Target"))
            {
                doesReachToTarget = true;
                Destroy(other.gameObject);
            }
        }
        #endregion

        #region Private Methods

        private void AI()
        {
            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    break;
                case UnitState.ALERT:
                    break;
                case UnitState.COMBAT:
                    break;
                case UnitState.CHEESE:
                    break;
                case UnitState.INSMOKE:
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

        }

        //특정 타겟(CAN, CHEESE, Player) 위치로 이동
        //우선순위: CHEESE > Player > CAN > Patrol Spot
        private void ChaseTarget()
        {
            //특이 사항 없음
            if (queue.Count() == 0)
            {

            }
            else
            { 
                //현재 추적하고 있는 타겟이 없을 때만 queue에서 정보를 받아온다.
                if (curTarget.code == WeaponCode.max)
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
                case WeaponCode.PLAYER:
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        Move();
                    break;
                case WeaponCode.CHEESE:
                    Move();
                    break;
                default:
                    break;
            }
        }

        private void Move()
        {
            if (curTarget.code != WeaponCode.max && !doesReachToTarget)
                agent.SetDestination(curTarget.pos);
            else if (doesReachToTarget)
            {
                if (!stayDelay.IsRunning)
                    stayDelay.Restart();

                if (checkStayDelay)
                {
                    InitCurTarget();
                    doesReachToTarget = false;
                }
            }
        }

        private void SetCurTarget()
        {
            curTarget = queue.Dequeue();
            Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);
        }

        private void InitCurTarget()
        {
            curTarget.code = WeaponCode.max;
            curTarget.pos = ValueCollections.InitPosition;
            stayDelay.Stop();
        }

        #endregion

        #region Public Methods

        public void Detect(WeaponCode code, Vector3 pos)
        {
            switch (code)
            {
                case WeaponCode.CAN:
                case WeaponCode.SMOKE:
                    unit.curUnitState = UnitState.IDLE;
                    unit.alertValue = AggroCollections.alertMin;
                    break;
                case WeaponCode.PLAYER:
                    unit.curUnitState = UnitState.COMBAT;
                    unit.alertValue = AggroCollections.combatMin;
                    break;
                case WeaponCode.CHEESE:
                    unit.curUnitState = UnitState.CHEESE;
                    unit.alertValue = AggroCollections.combatMin;
                    break;
                default:
                    break;
            }
                        
            queue.Enqueue(code, pos);
        }

        #endregion

    }
}

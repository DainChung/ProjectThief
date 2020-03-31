﻿using System.Diagnostics;
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
            public bool FindPlayer { get { return (queue[0].code == WeaponCode.PLAYER) || (queue[0].code == WeaponCode.PLAYERTRACK); } }

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
            private void Clear()
            {
                queue[0].code = WeaponCode.max;
                queue[0].pos.Set(-1, -1, -1);
                queue[1].code = WeaponCode.max;
                queue[1].pos.Set(-1, -1, -1);
                queue[2].code = WeaponCode.max;
                queue[2].pos.Set(-1, -1, -1);
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

        private Vector3 playerPosition = ValueCollections.initPos;

        #endregion

        #region Public Fields

        public Transform throwPos;
        public float moveSpeed { get { return (agent.velocity == Vector3.zero) ? 0.0f : 1.0f; } }
        public bool doesReachToTarget { set { _doesReachToTarget = value; } }

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
            radarMng.enabled = true;
        }
        

        void FixedUpdate()
        {
            if (!unit.lockControl)
            {
                if(unit.health > 0)
                    ChaseTargetBYQueue();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Target"))
            {
                _doesReachToTarget = true;
                Destroy(other.gameObject);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Target"))
            {
                _doesReachToTarget = true;
                Destroy(other.gameObject);
            }
        }

        #endregion

        #region Private Methods

        private void LookDir()
        {
            switch (unit.curLookDir)
            {
                case LookDirState.FINDPLAYER:
                    playerPosition = GameObject.FindWithTag("Player").transform.position;
                    transform.LookAt(playerPosition);
                    break;
                default:
                    playerPosition = ValueCollections.initPos;
                    break;
            }
        }

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
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        MovePlayerTrack();
                    break;
                case WeaponCode.PLAYER:
                    if (queue.FindCheese)
                        SetCurTarget();
                    else
                        MoveToPlayer();
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
            if (curTarget.code != WeaponCode.max && !_doesReachToTarget)
                agent.SetDestination(curTarget.pos);
            else if (_doesReachToTarget)
            {
                if (!stayDelay.IsRunning)
                    stayDelay.Restart();

                if (checkStayDelay)
                {
                    InitCurTarget();
                    _doesReachToTarget = false;
                }
            }
        }
        private void MovePlayerTrack()
        {
            if (!_doesReachToTarget)
                agent.SetDestination(playerPosition);
            else
            {
                _doesReachToTarget = false;
                InitCurTarget();
                MyDebug.Log("정지");
            }
        }
        private void MoveToPlayer()
        {
            if (!_doesReachToTarget)
                agent.SetDestination(playerPosition);
            else
            {
                _doesReachToTarget = false;
                InitCurTarget();
                playerPosition = ValueCollections.initPos;
                MyDebug.Log("정지");
            }
        }

        private void SetCurTarget()
        {
            MyDebug.Log("SetTarget");
            curTarget = queue.Dequeue();
            Instantiate(Resources.Load(FilePaths.AISystemPath + "Target") as GameObject, curTarget.pos, transform.rotation);
        }

        private void InitCurTarget()
        {
            curTarget.code = WeaponCode.max;
            curTarget.pos = ValueCollections.initPos;
            stayDelay.Stop();
        }

        private void EnemyAlertManager()
        {
            unit.AlertManager();

            switch (unit.curUnitState)
            {
                case UnitState.IDLE:
                    agent.speed = 4.0f;
                    radarMng.EnableEnemyRadar(true);
                    break;
                case UnitState.ALERT:
                    agent.speed = 2.5f;
                    radarMng.EnableEnemyRadar(true);
                    StartCoroutine(radarMng.Sample(playerPosition));
                    break;
                case UnitState.COMBAT:
                    agent.speed = 7.0f;
                    radarMng.EnableEnemyRadar(false);
                    break;
                default:
                    break;
            }

            MyDebug.Log(unit.curUnitState);
        }

        #endregion

        #region Public Methods

        public void Detect(WeaponCode code, Vector3 pos)
        {
            switch (code)
            {
                case WeaponCode.CAN:
                case WeaponCode.SMOKE:
                case WeaponCode.PLAYERTRACK:
                    if(unit.alertValue < AggroCollections.alertMin)
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

            EnemyAlertManager();
            MyDebug.Log("EnemySpeed: "+agent.speed);
                        
            queue.Enqueue(code, pos);
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

        #endregion

    }
}

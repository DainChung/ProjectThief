using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.Exceptions;
using Com.MyCompany.MyGame.GameSystem;

namespace Com.MyCompany.MyGame
{
    public class Unit : MonoBehaviour
    {
        #region SubClasses

        public class ThrowLineRenderer
        {
            private const float gravity = 9f;
            private const float throwPower = 12f;

            public Transform throwDestiPos;
            public LineRenderer lineRenderer;

            public ThrowLineRenderer(Transform throwDestiPos, LineRenderer lineRenderer)
            {
                this.throwDestiPos = throwDestiPos;
                this.lineRenderer = lineRenderer;

                throwDestiPos.GetComponent<MeshRenderer>().enabled = false;
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.positionCount = 27;

                lineRenderer.enabled = false;
            }

            //탄도방정식을 이용하여 궤적을 그릴 수 있도록 Vector3 값을 반환한다.
            //theta : 발사각도
            //t : 시간
            //eulerAngleY : 캐릭터가 바라보는 방향
            private Vector3 GetThrowLinePoint(float theta, float t, float eulerAngleY)
            {
                //시간 t로 x, y, z 값을 구한다.
                float x = MyMath.Cos(theta) * MyMath.Sin(eulerAngleY) * throwPower * t;
                float y = (0.95f * throwPower * MyMath.Sin(theta) - 0.545f * gravity * t) * t;
                float z = MyMath.Cos(theta) * MyMath.Cos(eulerAngleY) * throwPower * t;

                return new Vector3(x, y, z);
            }

            //theta : 발사각도
            //t : 시간
            //eulerAngleY : 캐릭터가 바라보는 방향
            public void Draw(float theta, Vector3 throwPos, float eulerAngleY)
            {
                if (!lineRenderer.enabled) //조준할 때만 LineRenderer를 활성화합니다.
                {
                    lineRenderer.enabled = true;
                    throwDestiPos.GetComponent<MeshRenderer>().enabled = true; //착탄지점을 보여줍니다.
                }

                theta *= (-1); //발사각도

                float t = 0.08f; //탄도방정식에 넣을 변수 t

                for (int index = 0; index < lineRenderer.positionCount; index++)
                {
                    lineRenderer.SetPosition(index, GetThrowLinePoint(theta, t, eulerAngleY) + throwPos);

                    //구조물과 닿는 지점부터 계산을 생략합니다.
                    if (index != 0 && CheckObject(index)) break;

                    t += 0.1f;
                }
            }

            public void HideLines()
            {
                lineRenderer.enabled = false;
                throwDestiPos.GetComponent<SphereCollider>().enabled = false;
                throwDestiPos.GetComponent<MeshRenderer>().enabled = false;
            }

            private bool CheckObject(int index)
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Linecast(lineRenderer.GetPosition(index - 1), lineRenderer.GetPosition(index), out hit))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Structure") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        throwDestiPos.position = hit.point;

                        lineRenderer.SetPosition(index, lineRenderer.GetPosition(index));
                        for (int i = index; i < lineRenderer.positionCount; i++)
                            lineRenderer.SetPosition(i, lineRenderer.GetPosition(index));

                        return true;
                    }
                }

                return false;
            }
        }

        public class StopwatchManager
        {
            //딜레이
            private List<Stopwatch> sw = new List<Stopwatch>();
            private List<long> swDelays = new List<long>();

            public Stopwatch throwAnimSW = new Stopwatch();
            public int secThrowAnimSW { get { return throwAnimSW.Elapsed.Seconds; } }

            public Stopwatch attackCountDelay = new Stopwatch();
            public int secAttackCountDelay { get { return attackCountDelay.Elapsed.Seconds; } }

            /// <summary>
            /// 다수의 StopWatch 사용
            /// </summary>
            /// <param name="delayTimes">.Length == swLength + 1</param>
            /// <param name="swLength"></param>
            public StopwatchManager(long[] delayTimes, int swLength)
            {
                for (int i = 0; i < swLength + 1; i++)
                {
                    swDelays.Add(new long());
                    swDelays[i] = delayTimes[i];

                    if (i < swLength)
                    {
                        sw.Add(new Stopwatch());
                        sw[i].Start();
                    }
                }

                throwAnimSW.Start();
                attackCountDelay.Start();
            }

            public void RestartSW(int index)
            {
                sw[index].Restart();
            }
            public void StopSW(int index)
            {
                sw[index].Stop();
            }
            public void ResetSW(int index)
            {
                sw[index].Reset();
            }
            public void ResetStopSW(int index)
            {
                sw[index].Reset();
                sw[index].Stop();
            }
            public bool IsRunningSW(int index)
            {
                return sw[index].IsRunning;
            }
            public void SWDelayManager()
            {
                for (int i = 0; i < sw.Count; i++)
                {
                    if (sw[i].IsRunning)
                    {
                        if (sw[i].Elapsed.Milliseconds >= swDelays[i])
                            sw[i].Stop();
                    }
                }
            }
            public bool SWDelayDone(WeaponCode code)
            {
                return sw[(int)(code)].ElapsedMilliseconds >= swDelays[(int)(code)];
            }
            public bool SWDelayDone(int index)
            {
                return sw[index].ElapsedMilliseconds >= swDelays[index];
            }
            public bool SWDelayDone(bool isPlayer)
            {
                if(isPlayer)
                    return sw[0].ElapsedMilliseconds >= swDelays[0];
                else
                    return sw[0].ElapsedMilliseconds >= swDelays[4];
            }
            public bool AttackCountDelayDone()
            {
                return secAttackCountDelay >= ValueCollections.attackCountDelay;
            }
        }

        public class UnitAnimationHelper
        {
            #region Private Fields

            private bool _isOnFloor = false;
            private bool _isWallClose = false;

            #endregion

            #region Public Fields

            public bool isOnFloor { get { return _isOnFloor; } set { _isOnFloor = value; } }
            public bool isWallClose { get { return _isWallClose; } set { _isWallClose = value; } }

            public Transform wallTransform;
            public Vector3 nearWallEndPos;
            public Vector3 wallEndToEndPos;

            #endregion

            public UnitAnimationHelper()
            {
                nearWallEndPos = Vector3.zero;
            }
        }

        #endregion

        #region Private Fields


        //발사각
        private Quaternion throwRot = new Quaternion();

        private UnitAnimationHelper unitAnimHelper = new UnitAnimationHelper();
        private Rigidbody rb;

        private AnimatorStateInfo currentLayerAnimInfo;

        //조준 후 발사되지 않는 경우 발사하도록 제어
        private bool _readyToThrowItem = false;
        //동시에 두번 발사 되는 현상 방지
        private bool _doubleThrowLock = false;

        private bool _lockControl = false;  //true면 유닛이 행동을 못하게 한다.
        private UnitStat unitStat;

        private UnitState _curUnitState = UnitState.IDLE;
        private LookDirState _curLookDir = LookDirState.IDLE;

        private CapsuleCollider collider;
        private CharacterController charController;
        private StageManager stageManager;

        #endregion

        #region Public Fields

        public float speed      { get { return unitStat.speed; } }      //기본 이동속도
        public float walkSpeed  { get { return unitStat.walkSpeed; } }  //걷기 이동속도
        public float coverSpeed { get { return unitStat.coverSpeed; } } //엄폐 이동속도
        public float health     { get { return unitStat.health; } }     //현재 체력
        public float maxHealth  { get { return unitStat.MaxHealth; } }  //최대 체력
        public float jumpPower  { get { return unitStat.jumpPower; } }  //점프력

        public UnitState curUnitState //유닛의 상태
        {
            get { return _curUnitState; }
            set { _curUnitState = value; }
        }
        public LookDirState curLookDir //유닛이 바라볼 방향
        {
            get { return _curLookDir; }
            set { _curLookDir = value; }
        }

        public Animator animator;
        public Transform throwDestiPos;

        public AttackCollider attackCollider; //공격할 때 사용하는 Collider

        public string unitCode; //유닛의 코드

        public UnitAnimationController unitAnimController;
        public ThrowLineRenderer throwLine; //물건을 던질 때 예상되는 궤적을 그린다.
        public StopwatchManager swManager;  //각 행동마다 딜레이를 주는 Stopwatch들을 관리한다.

        [HideInInspector] public UnitPose curUnitPose = UnitPose.MOD_RUN;
        [HideInInspector] public float alertValue = 0.0f;

        public bool readyToThrowItem { get { return _readyToThrowItem; } }
        public bool doubleThrowLock { get { return _doubleThrowLock; } }
        public AnimatorStateInfo standingLayerAnimInfo  { get { return animator.GetCurrentAnimatorStateInfo(AnimationLayers.Standing); }}
        public bool lockControl { get { return _lockControl; } set { _lockControl = value; } }
        #endregion

        #region MonoBehaviour Callbacks
        void Awake()
        {
            unitStat = new UnitStat(unitCode);
            unitAnimController = new UnitAnimationController(this, animator);

            long[] delays = { 200, 2000, 3000, 4000, 1000};
            swManager = new StopwatchManager(delays, 4);
        }

        void Start()
        {
            throwLine = new ThrowLineRenderer(throwDestiPos, GetComponent<LineRenderer>());
            throwLine.HideLines();

            attackCollider.InitAttackCollider(1);

            try { charController = GetComponent<CharacterController>(); }
            catch (System.Exception) { }

            try
            {
                collider = GetComponent<CapsuleCollider>();
                collider.enabled = true;
                rb = GetComponent<Rigidbody>();
                rb.useGravity = true;
            }
            catch (System.Exception) { }
        }

        void Update()
        {
            if (health > 0)
            {
                if (IsOnFloor())
                {
                    try
                    {
                        CheckExeption();
                        switch (curUnitPose)
                        {
                            case UnitPose.MOD_FALL:
                                UnitIsOnFloor();
                                break;
                            default:
                                break;
                        }
                    }
                    catch (FreezingUnitException){ UnFreezeUnit(); }
                }
                else
                    Fall();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Floor"))
                unitAnimHelper.isOnFloor = true;

            if (other.CompareTag("Wall"))
            {
                unitAnimHelper.isWallClose = true;
                unitAnimHelper.wallTransform = other.transform;
            }

            //if (other.CompareTag("WallRightEnd") && unitAnimHelper.isWallClose)
            //{
            //    animator.SetBool("IsWallRightEnd", true);
            //    unitAnimHelper.nearWallEndPos = other.GetComponent<WallEnd>().nearWallEndPos;
            //    unitAnimHelper.wallEndToEndPos = other.GetComponent<WallEnd>().wallEndToEndPos;
            //}

            //if (other.CompareTag("WallLeftEnd") && unitAnimHelper.isWallClose)
            //{
            //    animator.SetBool("IsWallLeftEnd", true);
            //    unitAnimHelper.nearWallEndPos = other.GetComponent<WallEnd>().nearWallEndPos;
            //    unitAnimHelper.wallEndToEndPos = other.GetComponent<WallEnd>().wallEndToEndPos;
            //}
        }
        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Floor"))
                unitAnimHelper.isOnFloor = true;
            if (other.CompareTag("Wall"))
            {
                unitAnimHelper.isWallClose = true;
                unitAnimHelper.wallTransform = other.transform;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Floor") && transform.CompareTag("Player"))
                unitAnimHelper.isOnFloor = false;

            if (other.CompareTag("Wall"))
                unitAnimHelper.isWallClose = false;

            //if (other.CompareTag("WallRightEnd"))
            //    animator.SetBool("IsWallRightEnd", false);

            //if (other.CompareTag("WallLeftEnd"))
            //    animator.SetBool("IsWallLeftEnd", false);
        }

        #endregion

        #region Private Methods

        private IEnumerator DelayPlayDeadAnim(int damage)
        {
            unitAnimController.TurnOffAllLayers();
            _curUnitState = UnitState.IDLE;
            alertValue = 0;
            try
            {
                if (GetComponent<UnityEngine.AI.NavMeshAgent>().enabled)
                    GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
            }
            catch (System.Exception) { }

            Transform icon = transform.Find("Icon");
            icon.GetComponent<SpriteRenderer>().enabled = false;
            icon.Find("DeathIcon").GetComponent<SpriteRenderer>().enabled = true;

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            if (gameObject.layer == PhysicsLayers.Enemy)
            {
                Stopwatch delay = new Stopwatch();
                delay.Start();
                while (delay.ElapsedMilliseconds <= 100)
                    yield return null;
                delay.Stop();
            }
            unitAnimController.PlayDeadAnim(damage);
            yield break;
        }

        private void UnFreezeUnit()
        {
            lockControl = false;
            animator.SetBool("IsAttack", false);
            animator.Play("Idle 0-0", AnimationLayers.Standing);
            swManager.RestartSW((int)WeaponCode.HAND);
            swManager.attackCountDelay.Restart();
            curUnitPose = UnitPose.MOD_RUN;
            curLookDir = LookDirState.IDLE;
        }
        private void CheckExeption()
        {
            ValidateException.ValidateFreezingAttackException(_lockControl, unitAnimController.currentAnimStateInfo, unitAnimController.currentAnimLayer);
            ValidateException.ValidateFreezingUnitException(animator.GetBool("IsAttack"), unitAnimController.currentAnimStateInfo, "HitReaction");
        }
        #endregion

        #region Public Methods

        public void Dead()
        {
            try
            {
                collider.enabled = false;
                rb.velocity = Vector3.zero;
                rb.useGravity = false;
            }
            catch (System.Exception) { }

            if (transform.CompareTag("Player")) SendMessage("ShowDeadWindow");
            else if (transform.gameObject.layer == PhysicsLayers.Enemy)
            {
                SendMessage("OffIndicator", "AssassinateIndicator");
                try { DestroyImmediate(transform.GetComponent<EnemyController>().rightArm.Find("WeaponCHEESE").gameObject); }
                catch (System.Exception) { }
            }

            Destroy(gameObject, ValueCollections.deadBodyRemainTime);
        }
        public void HitHealth(float damage, Vector3 pos)
        {
            if (IsOnFloor() && health > 0)
            {
                _curUnitState = UnitState.COMBAT;
                //일반 공격(damage > 0)
                if (damage > 0)
                {
                    if (damage >= health)
                    {
                        GameObject.Find("Manager").GetComponent<StageManager>().UpdateScore(Score.NORNALKILL);
                        _lockControl = true;
                        unitStat.SetHealth(0);
                        StartCoroutine(DelayPlayDeadAnim((int)damage));
                    }
                    else
                    {
                        unitStat.SetHealth(health - damage);

                        transform.LookAt(pos);
                        if (!standingLayerAnimInfo.IsName("HitReaction")) animator.Play("HitReaction", AnimationLayers.Standing);
                        animator.SetBool("IsHit", true);
                        alertValue = AggroCollections.combatMin;

                        try { transform.GetComponent<EnemyController>().EnemyAlertManager(); }
                        catch (NullReferenceException) { AlertManager(); }
                        _lockControl = true;
                    }
                }
                //암살(damage < 0), 암살은 Player만 사용가능
                else
                {
                    GameObject.Find("Manager").GetComponent<StageManager>().UpdateScore(Score.ASSASSINATE);
                    unitStat.SetHealth(0);
                    StartCoroutine(DelayPlayDeadAnim((int)damage));
                }

                if (transform.CompareTag("Player"))
                    SendMessage("UpdateHPBar", unitStat.hpRatio);                    
            }
        }

        public void AttackDefault(bool isPlayer)
        {
            if (swManager.SWDelayDone(isPlayer))
            {
                swManager.ResetSW(0);
                int attackCount = animator.GetInteger("AttackCount");

                if (!animator.GetCurrentAnimatorStateInfo(AnimationLayers.Standing).IsName("Attack " + attackCount.ToString()))
                {
                    //애니메이션 제어
                    unitAnimController.TurnOffAllLayers();

                    if (attackCount > 0) attackCount = 0;
                    else attackCount++;

                    animator.Play("Attack " + attackCount.ToString(), AnimationLayers.Standing);
                    animator.SetInteger("AttackCount", attackCount);
                    animator.SetBool("IsAttack", true);
                    curUnitPose = UnitPose.MOD_ATTACK;

                    //조작 제어
                    attackCollider.InitAttackCollider(1);
                    EnableAttackCollider(true);

                    ////위치 제어
                    //try { rb.AddForce(transform.forward * 2, ForceMode.Impulse); }
                    //catch (System.Exception) { GetComponent<CharacterController>().Move(transform.forward * Time.deltaTime); }
                }
                //attackSW[0]가 영구적으로 중지되는 경우 방지
                else if (!swManager.IsRunningSW(0))
                    swManager.RestartSW(0);
            }
        }

        public IEnumerator SetCoverPosition(bool isCovering)
        {
            Vector3 wallPos = WallTransform().position;
            Vector3 wallRight = WallTransform().right;

            float colliderHeight = charController.center.y;//transform.GetComponent<CapsuleCollider>().center.y;

            float newX, newZ;
            float alpha, beta;

            alpha = wallRight.z / wallRight.x;
            beta = wallPos.z - alpha * wallPos.x;

            //벽이 좌표평면에서 x = a 그래프와 유사한 경우 alpha값이 835만 정도가 나옴
            //=> alpha 값이 1000을 넘어서면 x = a 그래프 꼴로 인식하고 계산하도록 변경
            if (alpha < 1000)
            {
                // wallPos.z = wallPos.x * alpha + beta;    transform.pos.z = transform.pos.x / (-alpha) + gamma;
                // newZ = newX * alpha + beta;              newZ = newX / (-alpha) + gamma;
                newX = (alpha * transform.position.z + transform.position.x - alpha * beta) / (alpha * alpha + 1);
                newZ = alpha * newX + beta;
            }
            //x = a 꼴이면 유닛 위치의 z값은 그대로 놓고 x값만 변경시켜 벽에 밀착하도록 한다.
            else
            {
                newZ = transform.position.z;
                newX = wallPos.x;
            }

            //엄폐 상태가 되면
            if (isCovering)
            {
                //Collider를 조금 이동시켜서 애니메이션이 자연스럽게 보이도록 한다
                charController.center = new Vector3(0, colliderHeight, 0.7f);

                Vector3 newVector = new Vector3(newX, transform.position.y, newZ);

                int t = 0;

                while (Vector3.Distance(transform.position, newVector) >= 0.2)
                {
                    //Debug.Log(Vector3.Distance(transform.position, newVector));
                    transform.position = Vector3.Lerp(transform.position, newVector, Time.deltaTime * 10);
                    t++;
                    yield return null;
                }
            }
            //엄폐를 해제하면 Collider 위치를 초기화한다.
            else
            {
                charController.center = new Vector3(0, colliderHeight, 0);
            }
            collider.center = charController.center;

            yield break;
        }

            #region 던지기 동작 및 기능 제어

        //캐릭터가 아이템을 던지기 전에 조준하는 함수
        public void AttackPhaseAiming(Vector3 throwPos, Vector3 throwRotEuler, ref float unitSpeed)
        {
            animator.SetBool("IsThrowMode", true);
            _curLookDir = LookDirState.THROW;
            curUnitPose = UnitPose.MOD_THROW;

            //발사각 결정
            float theta = throwRotEuler.x;

            if (theta >= 334)
                theta = theta - 360;

            theta = theta - 35;

            throwRotEuler.Set(theta, throwRotEuler.y, 0);
            throwRot = Quaternion.Euler(throwRotEuler);

            //포물선 궤적 그리기
            throwLine.Draw(throwRotEuler.x, throwPos, transform.rotation.eulerAngles.y);

            //플레이어 캐릭터 속도 & 회전 & 애니메이션 관리
            unitSpeed = walkSpeed;

            animator.SetLayerWeight(AnimationLayers.Throw, 1);
            animator.SetFloat("ThrowAnimSpeed", 0.01f);
            if (swManager.secThrowAnimSW >= 1)
            {
                animator.Play("Throw", AnimationLayers.Throw, 0);
                swManager.throwAnimSW.Restart();
            }
            animator.SetFloat("ThrowAnimSpeed", 0);

            animator.SetBool("IsRunMode", false);

            _readyToThrowItem = true;
            _doubleThrowLock = true;
        }

        //캐릭터가 아이템을 투척하기 위한 함수
        public void AttackPhaseThrow(Vector3 throwPos, WeaponCode weapon, ref float unitSpeed)
        {
            animator.SetBool("IsThrowMode", false);
            animator.SetLayerWeight(AnimationLayers.Throw, 1);
            animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);
            animator.SetFloat("ThrowAnimSpeed", 1);

            InstantiateWeapon(weapon, throwPos, throwRot);

            //포물선 숨기기
            throwLine.HideLines();
            //엄폐 상태 아닐 때 수행
            if (!animator.GetBool("IsCovering"))
            {
                //애니메이션 및 속도 변경
                unitSpeed = speed;
                animator.SetBool("IsRunMode", true);
            }

            //Throw 애니메이션 수행
            animator.SetBool("ThrowItem", true);
            _readyToThrowItem = false;
            _doubleThrowLock = false;
        }

        //Throw 애니메이션 종료
        public void ResetThrowAnimation()
        {
            curUnitPose = UnitPose.MOD_RUN;

            swManager.throwAnimSW.Restart();

            animator.Play("Throw", AnimationLayers.Throw, 0.0f);
            animator.SetFloat("ThrowAnimSpeed", 0);
            animator.SetLayerWeight(AnimationLayers.Throw, 0);
            animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);
            animator.SetBool("ThrowItem", false);
        }

        public void InstantiateWeapon(WeaponCode weapon, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Instantiate(Resources.Load(FilePaths.weaponPath + weapon.ToString()) as GameObject, pos, rot) as GameObject;

            if (weapon != WeaponCode.max)
            {
                if (weapon != WeaponCode.SMOKE)
                    obj.GetComponent<WeaponThrow>().SetCode(weapon);
                else
                    obj.GetComponent<WeaponSmoke>().SetCode(weapon);
            }
            swManager.RestartSW((int)weapon);
        }

            #endregion

            #region UnitAnimationManager 변수 읽기
        public bool IsOnFloor()
        {
            return unitAnimHelper.isOnFloor;
        }
        public bool IsWallClose()
        {
            return unitAnimHelper.isWallClose;
        }
        public Vector3 NearWallEndPos()
        {
            return unitAnimHelper.nearWallEndPos;
        }
        public Vector3 WallEndToEndPos()
        {
            return unitAnimHelper.wallEndToEndPos;
        }
        public Transform WallTransform()
        {
            return unitAnimHelper.wallTransform;
        }
        #endregion

        //공격 판정 콜라이더 제어
        public void EnableAttackCollider(bool enable)
        {
            attackCollider.enableCollider = enable;

            if (attackCollider.damage < 0)
            {
                float val = (enable ? 1.0f : 0.0f);
                if (!enable)
                {
                    animator.Play("Walk", AnimationLayers.Assassinate, 0);
                    animator.SetBool("ReadyAssassinateAnim", false);
                }

                animator.SetLayerWeight(AnimationLayers.Assassinate, val);
                animator.SetFloat("AssassinateAnimSpeed", val);
                attackCollider.enableCollider = enable;
            }
        }

        //캐릭터가 추락 중일 때
        public void Fall()
        {
            animator.SetBool("IsCovering", false);
            animator.SetFloat("TurnRight", 0);
            animator.SetFloat("MoveSpeed", 0);
            animator.SetBool("IsFalling", true);
            curUnitPose = UnitPose.MOD_FALL;

            animator.speed = 1;
            _lockControl = false;
            animator.SetLayerWeight(AnimationLayers.Throw, 0);
            animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);
            throwLine.HideLines();
        }

        //캐릭터가 착지했을 때
        public void UnitIsOnFloor()
        {
            animator.SetBool("IsFalling", false);
            curUnitPose = UnitPose.MOD_RUN;
        }

        public void AddToAlertValue(float amount)
        {
            if (health > 0)
            {
                alertValue += amount;
                AlertManager();
            }
        }
        public void SetAlertValue(float amount)
        {
            if (health > 0)
            {
                alertValue = amount;
                AlertManager();
            }
        }
        /// <summary>
        /// alertValue에 따라 curUnitState를 변경
        /// </summary>
        public void AlertManager()
        {
            if (alertValue < AggroCollections.alertMin)
                curUnitState = UnitState.IDLE;
            else if (alertValue >= AggroCollections.alertMin && alertValue < AggroCollections.combatMin)
                curUnitState = UnitState.ALERT;
            else if (alertValue >= AggroCollections.combatMin && alertValue < AggroCollections.combatMin + 1)
                curUnitState = UnitState.COMBAT;
            else
            {
                alertValue = AggroCollections.combatMin + 1;
                curUnitState = UnitState.COMBAT;
            }

            curUnitState = UnitState.IDLE;
        }

        #endregion
    }
}

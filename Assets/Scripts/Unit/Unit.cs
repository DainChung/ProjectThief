using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

using Com.MyCompany.MyGame.Collections;

/*  Line Renderer 제어하는 스크립트를 Unit.cs로 옮길 것
 *  Stopwatch도 Unit.cs로 옮길 것
 *  AttackThrow와 관련 함수를 모두 Unit.cs로 옮길 것
 *  => 적 캐릭터도 AttackThrow함수를 이용할 수 있도록 변경할 것.
     */
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

            public ThrowLineRenderer()
            {
            }

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
            //cosY와 z 값을 변화시켜 모든 방향에 대해 탄도 궤적을 그리도록 변경할 것
            private Vector3 GetThrowLinePoint(float theta, float t, Vector3 throwPos, float eulerAngleY)
            {
                float cosY = Mathf.Cos(eulerAngleY * Mathf.Deg2Rad);
                float sinY = Mathf.Sin(eulerAngleY * Mathf.Deg2Rad);
                float cosEuler = Mathf.Cos(theta);
                float sinEuler = Mathf.Sin(theta);

                float x = cosEuler * sinY * throwPower * t;
                float y = (0.95f * throwPower * sinEuler - 0.5f * gravity * t * 1.09f) * t;  //0.95f, 1.09f는 임의로 추가한 계수, 이렇게 해야 궤적이 비슷해짐
                float z = cosEuler * cosY * throwPower * t;

                return (new Vector3(x, y, z) + throwPos);
            }

            //무언가를 던질때 궤적을 보여줌
            //예상 착탄지점을 원으로 표시해줄것 & 투척 후 선을 안 보이게 처리할 것
            public void Draw(float theta, Vector3 throwPos, float eulerAngleY)
            {
                if (!lineRenderer.enabled)
                {
                    lineRenderer.enabled = true;
                    throwDestiPos.GetComponent<MeshRenderer>().enabled = true;
                }

                theta = -(theta) * Mathf.Deg2Rad;

                //t가 0보다 조금 더 커야 궤적이 비슷해짐
                float t = 0.08f;

                for (int index = 0; index < lineRenderer.positionCount; index++)
                {
                    lineRenderer.SetPosition(index, GetThrowLinePoint(theta, t, throwPos, eulerAngleY));

                    if (index != 0)
                    {
                        RaycastHit hit = new RaycastHit();

                        //충돌이 발생한 부분부터 생략한다.
                        if (Physics.Linecast(lineRenderer.GetPosition(index - 1), lineRenderer.GetPosition(index), out hit))
                        {
                            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Structure"))
                            {
                                //착탄 지점 표시(나중에 투명하게 변경하거나 평면 원으로 표시할것)
                                throwDestiPos.position = hit.point;

                                lineRenderer.SetPosition(index, lineRenderer.GetPosition(index));
                                for (int i = index; i < lineRenderer.positionCount; i++)
                                    lineRenderer.SetPosition(i, lineRenderer.GetPosition(index));

                                break;
                            }
                        }
                    }

                    t += 0.1f;
                }
            }

            //궤적 숨기기
            public void HideLines()
            {
                lineRenderer.enabled = false;
                throwDestiPos.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        public class StopwatchManager
        {
            //무기 딜레이
            private Stopwatch[] attackSW = new Stopwatch[4];
            private int[] attackDelayTime = new int[4];

            public Stopwatch throwAnimSW = new Stopwatch();
            public int secThrowAnimSW { get { return throwAnimSW.Elapsed.Seconds; } }

            public void InitStopwatch()
            {
                attackDelayTime[0] = 1;
                attackDelayTime[1] = 2;
                attackDelayTime[2] = 3;
                attackDelayTime[3] = 4;

                attackSW[0] = new Stopwatch();
                attackSW[1] = new Stopwatch();
                attackSW[2] = new Stopwatch();
                attackSW[3] = new Stopwatch();

                attackSW[0].Start();
                attackSW[1].Start();
                attackSW[2].Start();
                attackSW[3].Start();

                throwAnimSW.Start();
            }

            public void RestartAttackStopwatch(int index)
            {
                attackSW[index].Restart();
            }

            public void AttackDelayManager()
            {
                for (int i = 0; i < attackSW.Length; i++)
                {
                    if (attackSW[i].IsRunning)
                    {
                        if (attackSW[i].Elapsed.Seconds >= attackDelayTime[i])
                            attackSW[i].Stop();
                    }
                }
            }

            public bool AttackDelayDone(WeaponCode code)
            {
                return attackSW[(int)(code)].Elapsed.Seconds >= attackDelayTime[(int)(code)];
            }
        }

        #endregion

        #region Private Fields

        private float _speed;
        private int _health;
        private float _jumpPower;
        private const string _weaponPath = "Weapons/Weapon";
        private bool _lockControl = false;

        //조준 후 발사되지 않는 경우 발사하도록 제어
        private bool _readyToThrowItem = false;
        //동시에 두번 발사 되는 현상 방지
        private bool _doubleThrowLock = false;

        //발사각
        private Quaternion throwRot = new Quaternion();

        #endregion

        #region Public Fields

        public float speed { get { return _speed; } set { _speed = value; } }
        public float walkSpeed { get { return 0.41f * _speed; } }
        public float coverSpeed { get { return 0.31f * speed; } }
        public int health { get { return _health; } }
        public float jumpPower { get { return _jumpPower; } }

        public string weaponPath { get { return _weaponPath; } }

        public bool lockControl { get{ return _lockControl; } }

        public bool readyToThrowItem { get { return _readyToThrowItem; } }
        public bool doubleThrowLock { get { return _doubleThrowLock; } }

        [HideInInspector]
        public UnitPose curUnitPose = UnitPose.MOD_RUN;
        //적 캐릭터만 사용
        [HideInInspector]
        public UnitState curUnitState = UnitState.IDLE;

        public Animator animator;
        public Transform throwDestiPos;

        public ThrowLineRenderer throwLine;
        public StopwatchManager swManager = new StopwatchManager();

        #endregion

        #region MonoBehaviour CallBacks
        void Awake()
        {
            _health = 1;
            _speed = 35.0f;
            _jumpPower = 10f;
        }
        // Start is called before the first frame update
        void Start()
        {
            throwLine = new ThrowLineRenderer(throwDestiPos, transform.GetComponent<LineRenderer>());
            swManager.InitStopwatch();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Private Methods


        #endregion

        #region Public Methods

        public void HitHealth(uint damage)
        {
            if (damage >= _health)
                _health = 0;
            else
                _health -= damage;
        }

        #region 엄폐 시 특별한 이동 제어
        public IEnumerator SetCoverPosition(Vector3 wallPos, Vector3 wallRight, bool isCovering)
        {
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
                transform.GetComponent<CapsuleCollider>().center = new Vector3(0, transform.GetComponent<CapsuleCollider>().center.y, 0.7f);

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
                transform.GetComponent<CapsuleCollider>().center = new Vector3(0, transform.GetComponent<CapsuleCollider>().center.y, 0);

            yield break;
        }

        //모퉁이에서 엄폐한 상태로 이동할 때 사용
        public IEnumerator SetCoverPosition(Vector3 destiPos, bool goRight, Vector3 subDestiPos)
        {
            float freezeY = transform.position.y;
            Vector3 freezeHeight = transform.position;
            Vector3 newLook = transform.right;

            if (goRight) newLook *= -1;

            //이동할 때 조작 방지
            _lockControl = true;

            transform.LookAt(transform.position + newLook, Vector3.up);

            //중간 지점까지 이동
            while (Vector3.Distance(transform.position, subDestiPos) >= 0.1)
            {
                transform.position = Vector3.Lerp(transform.position, subDestiPos, Time.deltaTime * 10);

                freezeHeight.Set(transform.position.x, freezeY, transform.position.z);
                transform.position = freezeHeight;
                yield return null;
            }

            //목표 지점까지 이동
            while (Vector3.Distance(transform.position, destiPos) >= 0.1)
            {
                transform.position = Vector3.Lerp(transform.position, destiPos, Time.deltaTime * 10);

                freezeHeight.Set(transform.position.x, freezeY, transform.position.z);
                transform.position = freezeHeight;
                yield return null;
            }

            _lockControl = false;
            yield break;
        }
        #endregion

        #region 던지기 동작 및 기능 제어

        //캐릭터가 아이템을 던지기 전에 조준하는 함수
        public void AttackPhaseAiming(Vector3 throwPos, Vector3 throwRotEuler, ref float unitSpeed, ref LookDirState lookDir)
        {
            animator.SetBool("IsThrowMode", true);
            lookDir = LookDirState.THROW;
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

            animator.SetLayerWeight(4, 1);
            animator.SetFloat("ThrowAnimSpeed", 0.01f);
            if (swManager.secThrowAnimSW >= 1)
            {
                animator.Play("Throw", 4, 0);
                swManager.throwAnimSW.Restart();
            }
            animator.SetFloat("ThrowAnimSpeed", 0);

            animator.SetBool("IsRunMode", false);

            _readyToThrowItem = true;
            _doubleThrowLock = true;
        }

        //캐릭터가 아이템을 투척하기 위한 함수
        public void AttackPhaseThrow(Vector3 throwPos, WeaponCode curWeapon, ref float unitSpeed)
        {
            animator.SetBool("IsThrowMode", false);
            animator.SetLayerWeight(4, 1);
            animator.SetLayerWeight(5, 0);
            animator.SetFloat("ThrowAnimSpeed", 1);

            Instantiate(Resources.Load(weaponPath + curWeapon.ToString()) as GameObject, throwPos, throwRot);

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

            swManager.RestartAttackStopwatch((int)curWeapon);
        }

        //Throw 애니메이션 종료
        public void ResetThrowAnimation()
        {
            curUnitPose = UnitPose.MOD_RUN;

            swManager.throwAnimSW.Restart();

            animator.Play("Throw", 4, 0.0f);
            animator.SetFloat("ThrowAnimSpeed", 0);
            animator.SetLayerWeight(4, 0);
            animator.SetLayerWeight(5, 0);
            animator.SetBool("ThrowItem", false);
        }

        #endregion

        //캐릭터가 추락 중일 때
        public void Fall()
        {
            animator.SetBool("IsFalling", true);
            curUnitPose = UnitPose.MOD_FALL;

            animator.SetLayerWeight(4, 0);
            animator.SetLayerWeight(5, 0);
            throwLine.HideLines();
        }

        //캐릭터가 땅 위에 있을 때
        public void UnitIsOnFloor()
        {
            animator.SetBool("IsFalling", false);
            curUnitPose = UnitPose.MOD_RUN;
        }

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class Unit : MonoBehaviour
    {
        #region Private Fields

        private float _speed;
        private uint _health;
        private float _jumpPower;
        private const string _weaponPath = "Weapons/";
        private bool _lockControl = false;

        #endregion

        #region Public Fields

        public float speed { get { return _speed; } set { _speed = value; } }
        public float walkSpeed { get { return 0.41f * _speed; } }
        public float coverSpeed { get { return 0.31f * speed; } }
        public uint health { get { return _health; } }
        public float jumpPower { get { return _jumpPower; } }

        public string weaponPath { get { return _weaponPath; } }

        public bool lockControl { get{ return _lockControl; } }

        [HideInInspector]
        public UnitPose curUnitPose = UnitPose.MOD_RUN;
        //적 캐릭터만 사용
        [HideInInspector]
        public UnitState curUnitState = UnitState.IDLE;

        //Animation Layer에 관한 정보
        public enum UnitPose
        {
            MOD_WALK = 0, MOD_RUN, MOD_CROUCH, MOD_COVERSTAND, MOD_COVERCROUCH, MOD_THROW, max
        }
        //적 캐릭터 상태에 관한 정보
        public enum UnitState
        {
            IDLE = 0, ALERT, COMBAT, max
        }

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

        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Public Methods

        public void HitHealth(uint damage)
        {
            if (damage >= _health)
                _health = 0;
            else
                _health -= damage;
        }

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

            //중간에 조작 방지
            _lockControl = true;

            transform.LookAt(transform.position + newLook, Vector3.up);

            while (Vector3.Distance(transform.position, subDestiPos) >= 0.1)
            {
                transform.position = Vector3.Lerp(transform.position, subDestiPos, Time.deltaTime * 10);

                freezeHeight.Set(transform.position.x, freezeY, transform.position.z);
                transform.position = freezeHeight;
                yield return null;
            }

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
    }
}

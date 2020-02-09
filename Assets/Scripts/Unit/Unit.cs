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

        #endregion

        #region Public Fields

        public float speed { get { return _speed; } set { _speed = value; } }
        public float walkSpeed { get { return 0.4f * _speed; } }
        public uint health { get { return _health; } }

        #endregion

        #region MonoBehaviour CallBacks
        void Awake()
        {
            _health = 1;
            _speed = 40.0f;
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
            //=> alpha 값이 100만을 넘어서면 x = a 그래프 꼴로 인식하고 계산하도록 변경
            if (alpha < 1000000)
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
                while (Vector3.Distance(transform.position, newVector) >= 0.2)
                {
                    transform.position = Vector3.Lerp(transform.position, newVector, Time.deltaTime * 10);
                    yield return null;
                }
            }
            //엄폐를 해제하면 Collider 위치를 초기화한다.
            else
                transform.GetComponent<CapsuleCollider>().center = new Vector3(0, transform.GetComponent<CapsuleCollider>().center.y, 0);

            yield break;
        }

        #endregion
    }
}

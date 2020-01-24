using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{

    public class Unit : MonoBehaviour
    {
        #region Private Values

        private float _speed;
        private uint _health;
        private bool _isOnFloor = false;

        #endregion

        #region Public Values

        public float speed { get { return _speed; } set { _speed = value; } }
        public uint health { get { return _health; } }
        public bool isOnFloor { get { return _isOnFloor; } }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            _health = 1;
            _speed = 20.0f;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerStay(Collider other)
        {
            if (other.tag.CompareTo("Floor") == 0)
                _isOnFloor = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.tag.CompareTo("Floor") == 0)
                _isOnFloor = false;
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

        /// <summary>
        /// 플레이어 캐릭터가 향할 위치를 반환하는 함수
        /// </summary>
        /// <param name="curPos"> 움직일 물체의 현재 위치 </param>
        /// <param name="dirTransform"> 방향의 기준이 되는 Transform(플레이어는 메인카메라) </param>
        /// <param name="vertical"> Input.GetAxis("Vertical") </param>
        /// <param name="horizontal"> Input.GetAxis("Horizontal") </param>
        /// <returns> 플레이어 캐릭터가 향할 위치 </returns>
        public Vector3 GetDestiPos(Vector3 curPos, Transform dirTransform, float vertical, float horizontal)
        {
            Vector3 result = curPos + (dirTransform.forward * vertical + dirTransform.right * horizontal) * _speed * Time.deltaTime;
            result.Set(result.x, curPos.y, result.z);

            return result;
        }

        /// <summary>
        /// 적 캐릭터가 향할 위치를 반환하는 함수
        /// </summary>
        /// <param name="vertical"> 전방 Or 후방 가중치 </param>
        /// <param name="horizontal"> 좌 Or 우 가중치 </param>
        /// <returns> 적 캐릭터가 향할 위치 </returns>
        public Vector3 GetDestiPos(float vertical, float horizontal)
        {
            Vector3 result = transform.position + (transform.forward * vertical + transform.right * horizontal) * _speed * Time.deltaTime;
            result.Set(result.x, transform.position.y, result.z);

            return result;
        }

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CameraWork : MonoBehaviour
    {
        #region Public Var

        public Vector3 cameraPos;
        public float smooth;

        #endregion

        #region Private Var

        private Transform player;
        private Vector3 destiPos;
        private float dist;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            destiPos = cameraPos;
        }

        void FixedUpdate()
        {
            FollowPlayer();
        }

        #endregion

        #region Private Methods

        private void FollowPlayer()
        {
            dist = Vector3.Distance(cameraPos, Vector3.zero);
            destiPos.Set(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * dist, cameraPos.y, Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * dist);
            destiPos = player.position - destiPos;
            destiPos.Set(destiPos.x, player.position.y + cameraPos.y, destiPos.z);

            //카메라 회전을 감안해서 플레이어 캐릭터를 따라다님
            transform.position = Vector3.Lerp(transform.position, destiPos, Time.deltaTime * smooth * player.GetComponent<Unit>().speed);
            //마우스 움직임에 따라 카메라 회전
            //transform.RotateAround(player.position, player.up, Time.deltaTime);
        }

        #endregion
    }
}

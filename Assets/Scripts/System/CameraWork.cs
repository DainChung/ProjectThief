using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Windows.Forms;

namespace Com.MyCompany.MyGame
{
    public class CameraWork : MonoBehaviour
    {
        #region Public Var

        public Vector3 cameraPos;
        public float smooth;

        public float rotationSpeed;

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

        //프레임과 상관없이 일정 시간마다 호출
        void FixedUpdate()
        {
            //마우스를 움직이면 카메라 회전
            CameraRotation(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            //플레이어 캐릭터를 따라다님
            FollowPlayer();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        //카메라 위치 조정 부분을 해결하면 회전 관련 문제가 해결됨
        private void FollowPlayer()
        {
            //카메라 위치 계산
            dist = Vector3.Distance(cameraPos, Vector3.zero);
            float cameraYValue = Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.x);

            destiPos.Set(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * dist * cameraYValue,
                        0f,
                        Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * dist * cameraYValue);

            destiPos = player.position - destiPos;
            destiPos.Set(destiPos.x, player.position.y + cameraPos.y * (1 + Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.x) * 1.5f), destiPos.z);

            //카메라 회전을 감안해서 플레이어 캐릭터를 따라다님 => Vector3.Lerp 말고 다른 방식으로 조작해야 카메라가 벽을 못 뚫게 할 수 있음
            transform.position = Vector3.Lerp(transform.position, destiPos, Time.deltaTime * smooth * player.GetComponent<Unit>().speed);
        }

        private void CameraRotation(float x, float y)
        {
            //x값 보정
            if (x != 0) x = x > 0 ? rotationSpeed : -rotationSpeed;
            //y값 보정
            if (y != 0) y = y > 0 ? rotationSpeed : -rotationSpeed;

            //플레이어를 중심으로 Y축 회전
            transform.RotateAround(player.position, player.up, x);
            //카메라의 X축 회전
            transform.RotateAround(player.position, transform.right, y);

            Vector3 curEulerAngle = transform.eulerAngles;

            //최대 회전 제한, 내려다 볼 수 있는 최대 각도 60도, 올려다 볼 수 있는 최소 각도 -25도, z축 회전 방지
            if (transform.eulerAngles.x > 60 && transform.eulerAngles.x < 90)
            {
                transform.rotation = Quaternion.Euler(60f, curEulerAngle.y, 0);
            }
            else if (transform.eulerAngles.x > 90 && transform.eulerAngles.x < 335)
            {
                transform.rotation = Quaternion.Euler(335f, curEulerAngle.y, 0);
            }
            //최대 각도, 최소 각도 도달 못해도 z축 회전하는 경우 방지
            else
            {
                transform.rotation = Quaternion.Euler(curEulerAngle.x, curEulerAngle.y, 0);
            }
        }

        #endregion
    }
}

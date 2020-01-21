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

       // [HideInInspector]
        public float rotationSpeed = 100f;

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
            //플레이어 캐릭터를 따라다님
            FollowPlayer();

            //마우스를 움직이면 카메라 회전
            CameraRotation(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        #endregion

        #region Private Methods

        private void FollowPlayer()
        {
            //카메라 X축 회전에 관한 값을 destiPos.y에 적용해야 됨
            dist = Vector3.Distance(cameraPos, Vector3.zero);
            destiPos.Set(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * dist, cameraPos.y, Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * dist);
            destiPos = player.position - destiPos;
            destiPos.Set(destiPos.x, player.position.y + cameraPos.y, destiPos.z);

            //카메라 회전을 감안해서 플레이어 캐릭터를 따라다님
            transform.position = Vector3.Lerp(transform.position, destiPos, Time.deltaTime * smooth * player.GetComponent<Unit>().speed);
        }

        private void CameraRotation(float x, float y)
        {
            //x값 보정
            if (x != 0) x = x > 0 ? rotationSpeed : -rotationSpeed;

            //최대 회전 제한, 내려다 볼 수 있는 최대 각도 60도, 올려다 볼 수 있는 최소 각도 -15도
            if (transform.eulerAngles.x > 60 && transform.eulerAngles.x < 90)
            {
                y = -0.1f;
            }
            else if (transform.eulerAngles.x > 90 && transform.eulerAngles.x < 345)
            {
                y = 0.1f;
            }
            else
            {
                //y값 보정
                if (y != 0) y = y > 0 ? rotationSpeed : -rotationSpeed;
            }

            y = 0;
            //플레이어를 중심으로 Y축 회전
            transform.RotateAround(player.position, player.up, x);

            //카메라의 X축 회전
            transform.RotateAround(player.position, transform.right, y);
        }

        //private void CameraRotation_OLD()
        //{
        //    rotationAngle = SetRotationAngle(Camera.main.ScreenToViewportPoint(Input.mousePosition));
        //    //마우스를 좌우로 움직이면 플레이어 캐릭터의 y축(Local Axis)을 중심으로 rotationAngle.x만큼 회전
        //    transform.RotateAround(player.position, player.up, rotationAngle.x);
        //    //마우스를 상하로 움직이면 플레이어 캐릭터의 x축(Local Axis)을 중심으로 rotationAngle.y만큼 회전
        //    transform.RotateAround(player.position, player.right, rotationAngle.y);

        //    //플레이어 캐릭터의 x축(Local Axis) 중심으로 회전할 때 각도 제한 필요 (Max == 50 && min == 0) || (Max == 360 && min == 330)
        //    //플레이어 캐릭터의 아래쪽을 바라볼때 카메라를 접근시켜야됨
            
        //}

        private Vector2 SetRotationAngle(Vector3 mouseViewportPoint)
        {
            Vector2 result = new Vector2();
            float mouseX = mouseViewportPoint.x - 0.5f;
            float mouseY = mouseViewportPoint.y - 0.5f;

            if ((mouseX < 0.45f && mouseX >= 0) || (mouseX > -0.45f && mouseX <= 0)) mouseX = 0;
            else mouseX = Mathf.Clamp(mouseX, -0.5f, 0.5f);

            if ((mouseY < 0.45f && mouseY >= 0) || (mouseY > -0.45f && mouseY <= 0)) mouseY = 0;
            else mouseY = Mathf.Clamp(mouseY, -0.5f, 0.5f);

            result.Set(mouseX * rotationSpeed * Time.deltaTime, mouseY * rotationSpeed * Time.deltaTime);

            return result;
        }

        #endregion
    }
}

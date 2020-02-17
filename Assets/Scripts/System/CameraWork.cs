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

        private LineRenderer lineRenderer;
        private const float gravity = 9f;
        private const float throwPower = 12f;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            destiPos = cameraPos;

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 27;

            lineRenderer.enabled = false;
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

        //무언가를 던질때 궤적을 보여줌
        //예상 착탄지점을 원으로 표시해줄것 & 투척 후 선을 안 보이게 처리할 것
        public void ThrowLineRenderer(float theta, Vector3 throwPos)
        {
            if (!lineRenderer.enabled)
                lineRenderer.enabled = true;

            theta = -(theta) * Mathf.Deg2Rad;

            //t가 0보다 조금 더 커야 궤적이 비슷해짐
            float t = 0.08f;

            for (int index = 0; index < lineRenderer.positionCount; index++)
            {
                lineRenderer.SetPosition(index, GetThrowLinePoint(theta, t, throwPos));
                t += 0.1f;

                //지표면 아래는 대부분 생략한다.
                if (lineRenderer.GetPosition(index).y < -2)
                {
                    lineRenderer.SetPosition(index, lineRenderer.GetPosition(index - 1));
                    for (int i = index; i < lineRenderer.positionCount; i++)
                        lineRenderer.SetPosition(i, lineRenderer.GetPosition(index));

                    break;
                }
            }
        }

        public void HideLines()
        {
            lineRenderer.enabled = false;
        }

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

        //탄도방정식을 이용하여 궤적을 그릴 수 있도록 Vector3 값을 반환한다.
        //cosY와 z 값을 변화시켜 모든 방향에 대해 탄도 궤적을 그리도록 변경할 것
        private Vector3 GetThrowLinePoint(float theta, float t, Vector3 throwPos)
        {
            float cosY = Mathf.Cos(transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            float sinY = Mathf.Sin(transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            float cosEuler = Mathf.Cos(theta);
            float sinEuler = Mathf.Sin(theta);

            float x = cosEuler * sinY * throwPower * t;
            float y = (0.95f * throwPower * sinEuler - 0.5f * gravity * t * 1.09f) * t;  //0.95f, 1.09f는 임의로 추가한 계수, 이렇게 해야 궤적이 비슷해짐
            float z = cosEuler * cosY * throwPower * t;

            return (new Vector3(x, y, z) + throwPos);
        }

        #endregion
    }
}

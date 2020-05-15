using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Windows.Forms;
using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class CameraWork : MonoBehaviour
    {
        #region Public Fields

        public Vector3 cameraPos;
        public float smooth;

        public float rotationSpeed;

        #endregion

        #region Protected Fields
        protected Transform player;
        protected Vector3 destiPos;
        protected Vector3 v = Vector3.zero;
        protected float maxDist;
        protected float dist;
        #endregion

        #region Private Fields
        private Vector3 height = new Vector3(0, 1, 0);
        #endregion

        #region Protected Methods

        protected virtual void FollowPlayer()
        {
            destiPos = GetPositionBYDist(dist);
            transform.position = Vector3.SmoothDamp(transform.position, destiPos, ref v, smooth);
        }

        protected virtual void CameraRotation(float x, float y)
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

            //최대 회전 제한, 내려다 볼 수 있는 최대 각도 60도, 올려다 볼 수 있는 최소 각도 -15도, z축 회전 방지
            if (transform.eulerAngles.x > 60 && transform.eulerAngles.x < 90)
            {
                transform.rotation = Quaternion.Euler(60f, curEulerAngle.y, 0);
            }
            else if (transform.eulerAngles.x > 90 && transform.eulerAngles.x < 345)
            {
                transform.rotation = Quaternion.Euler(345f, curEulerAngle.y, 0);
            }
            //최대 각도, 최소 각도 도달 못해도 z축 회전하는 경우 방지
            else
            {
                transform.rotation = Quaternion.Euler(curEulerAngle.x, curEulerAngle.y, 0);
            }
        }

        protected void CheckStructure()
        {
            Ray ray = new Ray(player.position + height, GetPositionBYDist(maxDist) - player.position - height);
            Debug.DrawRay(player.position + height, GetPositionBYDist(maxDist) - player.position - height);
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDist, 1 << PhysicsLayers.Structure);

            dist = maxDist;
            foreach (RaycastHit hit in hits)
                if (dist > hit.distance - 0.5f) dist = hit.distance - 0.5f;

            dist = Mathf.Clamp(dist, 0.8f, maxDist);
        }
        #endregion

        #region Private Methods

        private Vector3 GetPositionBYDist(float distance)
        {
            Vector3 result = Vector3.zero;

            distance = Mathf.Clamp(distance, 0.5f, maxDist);
            float cameraYValue = Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.x);

            result.Set(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * distance * cameraYValue,
                        0f,
                        Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * distance * cameraYValue);

            result = player.position - result;
            result.Set(result.x, player.position.y + cameraPos.y * (1 + Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.x) * 3f * (distance / maxDist)), result.z);

            return result;
        }

        #endregion
    }
}

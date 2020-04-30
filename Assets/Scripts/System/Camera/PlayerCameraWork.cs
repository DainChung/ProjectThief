using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Windows.Forms;

namespace Com.MyCompany.MyGame
{
    public class PlayerCameraWork : CameraWork
    {
        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;

            destiPos = cameraPos;
        }

        void FixedUpdate()
        {
            //마우스를 움직이면 카메라 회전
            CameraRotation(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            //플레이어 캐릭터를 따라다님
            FollowPlayer();
        }
    }
}

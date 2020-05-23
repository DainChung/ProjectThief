﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Windows.Forms;

namespace Com.MyCompany.MyGame
{
    public class PlayerCameraWork : CameraWork
    {
        private float _maxDist;

        // Start is called before the first frame update
        new void Start()
        {
            player = GameObject.FindWithTag("Player").transform;
            destiPos = cameraPos;

            maxDist = Vector3.Distance(cameraPos, Vector3.zero);
            base.Start();
        }

        void FixedUpdate()
        {
            CheckStructure();
            //마우스를 움직이면 카메라 회전
            CameraRotation(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            //플레이어 캐릭터를 따라다님
            FollowPlayer();

            Zoom(Input.mouseScrollDelta.y);
        }
    }
}

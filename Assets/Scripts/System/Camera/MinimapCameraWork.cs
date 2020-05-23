﻿using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.GameSystem;
using Com.MyCompany.MyGame.UI;

namespace Com.MyCompany.MyGame
{
    public class MinimapCameraWork : CameraWork
    {
        private UIManager uiManager;
        private Transform mainCamTransform;
        private Vector3 destiEuler = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            mainCamTransform = Camera.main.transform;
            player = GameObject.FindGameObjectWithTag("Player").transform;
            uiManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<UIManager>();

            destiPos = cameraPos;
            ChangeFloor(1);
            //transform.GetComponent<Camera>().cullingMask = (1 << LayerMask.NameToLayer("Default"));
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            FollowPlayer();
            CameraRotation();
        }

        protected override void FollowPlayer()
        {
            destiPos.Set(player.position.x, cameraPos.y, player.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, destiPos, ref v, smooth);
        }

        private void CameraRotation()
        {
            destiEuler.Set(90f, mainCamTransform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Euler(destiEuler);
        }

        public void ChangeFloor(int floorLayer)
        {
            int newCullingMask = 0;

            UIController uiFloor = uiManager.GetUIController("Window_MiniMap");
            switch (floorLayer)
            {
                case 1:
                    newCullingMask = UILayers.MiniMap1F;
                    uiFloor.SetText("1 F", "Text");
                    break;
                case 2:
                    newCullingMask = UILayers.MiniMap2F;
                    uiFloor.SetText("2 F", "Text");
                    break;
                case 3:
                    newCullingMask = UILayers.MiniMap3F;
                    uiFloor.SetText("3 F", "Text");
                    break;
                case 4:
                    newCullingMask = UILayers.MiniMap4F;
                    uiFloor.SetText("4 F", "Text");
                    break;
                default:
                    break;
            }

            newCullingMask = 1 << newCullingMask | 1 << UILayers.MiniMap;

            GetComponent<Camera>().cullingMask = newCullingMask;
        }
    }
}

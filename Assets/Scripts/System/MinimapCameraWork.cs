using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using System.Windows.Forms;

namespace Com.MyCompany.MyGame
{
    public class MinimapCameraWork : CameraWork
    {
        private Transform mainCamTransform;
        private Vector3 destiEuler = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            ChangeFloor(1);
            mainCamTransform = Camera.main.transform;
            player = GameObject.FindGameObjectWithTag("Player").transform;

            destiPos = cameraPos;

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

            switch (floorLayer)
            {
                case 1:
                    newCullingMask = UILayers.MiniMap1F;
                    break;
                case 2:
                    newCullingMask = UILayers.MiniMap2F;
                    break;
                case 3:
                    newCullingMask = UILayers.MiniMap3F;
                    break;
                case 4:
                    newCullingMask = UILayers.MiniMap4F;
                    break;
                default:
                    break;
            }

            newCullingMask = 1 << newCullingMask | 1 << UILayers.MiniMap;

            GetComponent<Camera>().cullingMask = newCullingMask;
        }
    }
}

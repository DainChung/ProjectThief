using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            mainCamTransform = Camera.main.transform;
            player = GameObject.FindGameObjectWithTag("Player").transform;

            destiPos = cameraPos;
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
    }
}

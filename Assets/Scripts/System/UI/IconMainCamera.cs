using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class IconMainCamera : UI
    {
        private Transform mainCamTransform;

        void Start()
        {
            mainCamTransform = Camera.main.transform;
        }

        void FixedUpdate()
        {
            Rotate(mainCamTransform.rotation.eulerAngles.y);
        }
    }
}

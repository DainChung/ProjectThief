using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class Icon : MonoBehaviour
    {
        private Transform miniMapCameraTransform;

        // Start is called before the first frame update
        void Start()
        {
            miniMapCameraTransform = GameObject.Find("MinimapCamera").transform;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            transform.rotation = Quaternion.Euler(-90, miniMapCameraTransform.rotation.eulerAngles.y + 180, 0);
        }
    }
}

using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class Indicator : UI
    {
        public Transform target = null;
        private Vector3 pos { get { return (target == null ? Vector3.zero : target.position); } }

        void Start()
        {
            Transform canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            Transform uiCam = GameObject.Find("UICamera").transform;
            base.InitMaxWMaxH(canvas.TransformVector(transform.position) / uiCam.localScale.x);
        }

        void Update()
        {
            base.Move(pos);
        }
    }
}

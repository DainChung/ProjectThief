using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class Indicator : UI
    {
        public Transform target = null;
        private Vector3 pos { get { return (target == null ? Collections.ValueCollections.initPos : target.position); } }

        private UIController uiController;

        void Start()
        {
            Transform canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            Transform uiCam = GameObject.Find("UICamera").transform;
            uiController = GetComponent<UIController>();
            base.InitMaxWMaxH(canvas.TransformVector(transform.position) / uiCam.localScale.x);
        }

        void Update()
        {
            if (pos != Collections.ValueCollections.initPos)
                base.Move(pos);
            else
                uiController.OnOffAll(false);
        }
    }
}

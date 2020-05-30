using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class Indicator : UI
    {
        public Transform target = null;
        private Vector3 pos { get { return (target == null ? Vector3.zero : target.position); } }

        void Update()
        {
            base.Move(pos);
        }
    }
}

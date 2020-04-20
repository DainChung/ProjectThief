using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class Indicator : UI
    {
        [HideInInspector]   public Transform target;

        void Awake()
        {
            if (target == null) this.enabled = false;
        }
        void Update()
        {
            if (target != null) base.Move(target.position);
        }
    }
}

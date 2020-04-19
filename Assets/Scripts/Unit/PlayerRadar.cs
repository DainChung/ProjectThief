using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class PlayerRadar : MonoBehaviour
    {
        private PlayerController playerController;

        void Start()
        {
            playerController = transform.parent.GetComponent<PlayerController>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
                playerController.SetNeareastItem(other, true);
        }
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
                playerController.SetNeareastItem(other, true);
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
                playerController.SetNeareastItem(other, false);
        }
    }

}

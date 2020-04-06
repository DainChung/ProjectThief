using System.Collections;
using System.Collections.Generic;
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
                playerController.nearestItem.Set(other.GetComponent<Item>(), other.transform.position);
        }
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
                playerController.nearestItem.Set(other.GetComponent<Item>(), other.transform.position);
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
                playerController.nearestItem.Delete(other.GetInstanceID());
        }
    }

}

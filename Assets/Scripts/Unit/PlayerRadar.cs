using System.Collections;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class PlayerRadar : MonoBehaviour
    {
        private PlayerController playerController;
        private bool canGetItem = false;

        void Start()
        {
            playerController = transform.parent.GetComponent<PlayerController>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
            {
                CheckStructure(other.transform.position);
                playerController.SetNeareastItem(other, canGetItem);
            }
        }
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
            {
                CheckStructure(other.transform.position);
                playerController.SetNeareastItem(other, canGetItem);
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Item)
            {
                canGetItem = true;
                playerController.SetNeareastItem(other, false);
            }
        }

        private void CheckStructure(Vector3 itemPos)
        {
            canGetItem = !(Physics.Raycast(transform.position, itemPos - transform.position, Vector3.Distance(itemPos, transform.position), 1 << PhysicsLayers.Structure));
        }
    }

}

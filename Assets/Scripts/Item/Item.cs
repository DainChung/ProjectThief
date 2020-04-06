using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Item : MonoBehaviour
    {
        private ItemCode _code = ItemCode.max;
        public ItemCode code { get { return _code; } }

        private bool nearToPlayer = false;

        public void SetItem(WeaponCode weaponCode)
        {
            if (_code == ItemCode.max)
                _code = EnumCollections.ConvertWeaponToItem(weaponCode);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Player)
                nearToPlayer = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Player)
                nearToPlayer = false;
        }

        public void SeenByCamera(bool isSeen)
        {
            if (isSeen && nearToPlayer)
            {
                Debug.Log("주울 수 있다는 적당한 UI 활성화");
            }
            else
            {
                Debug.Log("주울 수 있다는 적당한 UI를 지울것");
            }
        }
    }
}

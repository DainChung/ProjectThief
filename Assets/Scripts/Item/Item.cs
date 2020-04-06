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

        public void SetItem(WeaponCode weaponCode)
        {
            if (_code == ItemCode.max)
                _code = EnumCollections.ConvertWeaponToItem(weaponCode);
        }
    }
}

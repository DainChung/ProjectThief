using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.MyCompany.MyGame.Collections
{
    #region Enums
    public enum Item
    {
        GOLD = 0, CAN, DONUT, SMOKE, max
    }

    public enum WeaponCode
    {
        HAND = 0, CAN, DONUT, SMOKE, max
    }

    //Animation Layer에 관한 정보
    public enum UnitPose
    {
        MOD_WALK = 0, MOD_RUN, MOD_CROUCH, MOD_COVERSTAND, MOD_COVERCROUCH, MOD_THROW,
        MOD_THROWEND, max
    }
    //적 캐릭터 상태에 관한 정보
    public enum UnitState
    {
        IDLE = 0, ALERT, COMBAT, max
    }
    #endregion

    public static class EnumCollections
    {
        /// <summary>
        /// 서로 다른 임의의 Enum을 문자열로 변환해서 비교하는 함수
        /// </summary>
        /// <typeparam name="TEnum1">임의의 Enum1</typeparam>
        /// <typeparam name="TEnum2">임의의 Enum2</typeparam>
        /// <param name="tEnum1">비교할 Enum1</param>
        /// <param name="tEnum2">비교할 Enum2</param>
        /// <returns></returns>
        public static int CompareEnum<TEnum1, TEnum2>(TEnum1 tEnum1, TEnum2 tEnum2)
            where TEnum1 : struct, IConvertible, IComparable, IFormattable
            where TEnum2 : struct, IConvertible, IComparable, IFormattable
        {
            return tEnum1.ToString().CompareTo(tEnum2.ToString());
        }
    }

    public static class TransformCollections
    {
        #region Weapons
        //연막탄 위치 벡터
        private static Vector3 _WeaponSmokeVector3 = new Vector3(0, 1.2f, 0);
        public static Vector3 weaponSmokeVec { get { return _WeaponSmokeVector3; } }

        //연막탄 방향
        private static Quaternion _WeaponSmokeQuat = new Quaternion(-0.7f, 0, 0, 0.7f);
        public static Quaternion weaponSmokeQuat { get{ return _WeaponSmokeQuat; } }
        #endregion
    }

    public static class AnimationCollections
    {
        #region
        
        #endregion
    }
}

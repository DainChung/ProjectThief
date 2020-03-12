using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.MyCompany.MyGame.Collections
{

    #region Enums
    public enum Item
    {
        GOLD = 0, CAN, CHEESE, SMOKE, max
    }

    public enum WeaponCode
    {
        HAND = 0, CAN, CHEESE, SMOKE, max
    }

    //Animation Layer에 관한 정보
    public enum UnitPose
    {
        MOD_WALK = 0, MOD_RUN, MOD_CROUCH, MOD_COVERSTAND, MOD_COVERCROUCH, MOD_THROW,
        MOD_THROWEND, MOD_FALL, max
    }
    //적 캐릭터 상태에 관한 정보
    public enum UnitState
    {
        IDLE = 0, ALERT, COMBAT, CHEESE, max
    }
    //플레이어 캐릭터가 바라보는 방향
    public enum LookDirState
    {
        IDLE = 0, COVER, THROW, max
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

    public static class ValueCollections
    {
        private static int[] _itemMaxAmount = { 5, 3, 2 };
        public static int[] itemMaxAmount { get { return _itemMaxAmount; } }

        #region Weapons
        //연막탄 위치 벡터
        private static Vector3 _WeaponSmokeVector3 = new Vector3(0, 1.2f, 0);
        public static Vector3 weaponSmokeVec { get { return _WeaponSmokeVector3; } }

        //연막탄 방향
        private static Quaternion _WeaponSmokeQuat = new Quaternion(-0.7f, 0, 0, 0.7f);
        public static Quaternion weaponSmokeQuat { get { return _WeaponSmokeQuat; } }
        #endregion
    }

    public static class FilePaths
    {
        private static string _weaponPath = "Weapons/Weapon";

        public static string weaponPath { get { return _weaponPath; } }
    }

    public static class AggroCollections
    {
        private static float _aggroRun = 0.1f;
        private static float _aggroWalk = 0.05f;
        private static float _aggroCrouch = 0.01f;

        //1.0f 이상이면 curUnitState = ALERT
        private static float _alertMin = 1.0f;
        //2.0f 이상이면 curUnitState = COMBAT
        private static float _combatMin = 2.0f;


        public static float aggroRun { get { return _aggroRun; } }
        public static float aggroWalk { get { return _aggroWalk; } }
        public static float aggroCrouch { get { return _aggroCrouch; } }

        public static float alertMin { get { return _alertMin; } }
        public static float combatMin { get { return _combatMin; } }
    }

    //애니메이션 레이어
    public static class AnimationLayers
    {
        private static int _standing = 0;
        private static int _crouch = 1;
        private static int _coverStanding = 2;
        private static int _coverCrouch = 3;
        private static int _throw = 4;
        private static int _throwMove = 5;

        public static int Standing { get { return _standing; } }
        public static int Crouch { get { return _crouch; } }
        public static int CoverStanding { get { return _coverStanding; } }
        public static int CoverCrouch { get { return _coverCrouch; } }
        public static int Throw { get { return _throw; } }
        public static int ThrowMove { get { return _throwMove; } }
    }
}

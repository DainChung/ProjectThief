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

    //Enemy가 추적할 때 사용하기도 함
    public enum WeaponCode
    {
        HAND = 0, CAN, CHEESE, SMOKE, PLAYER, PLAYERTRACK, max
    }

    //Animation Layer에 관한 정보
    public enum UnitPose
    {
        MOD_WALK = 0, MOD_RUN, MOD_CROUCH, MOD_COVERSTAND, MOD_COVERCROUCH, MOD_THROW,
        MOD_THROWEND, MOD_FALL, MOD_INSMOKE, MOD_SLEEP, MOD_ATTACK, max
    }
    //적 캐릭터 상태에 관한 정보
    public enum UnitState
    {
        IDLE = 0, ALERT, COMBAT, CHEESE, INSMOKE, SLEEP, ASSASSINATE, max
    }
    //캐릭터가 바라보는 방향
    public enum LookDirState
    {
        IDLE = 0, COVER, THROW, FINDPLAYER, max
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

        #region Weapons Init Values
        //연막탄 위치 벡터
        private static Vector3 _WeaponSmokeVector3 = new Vector3(0, 1.2f, 0);
        /// <summary>
        /// Vector3(0, 1.2f, 0)
        /// </summary>
        public static Vector3 weaponSmokeVec { get { return _WeaponSmokeVector3; } }

        //연막탄 방향
        private static Quaternion _WeaponSmokeQuat = new Quaternion(-0.7f, 0, 0, 0.7f);
        /// <summary>
        /// Quaternion(-0.7f, 0, 0, 0.7f)
        /// </summary>
        public static Quaternion weaponSmokeQuat { get { return _WeaponSmokeQuat; } }
        #endregion

        #region Basic Init Vectors Values

        private static Vector3 _initPos = new Vector3(9999f, 9999f, 9999f);
        public static Vector3 initPos { get { return _initPos; } }

        #endregion

        #region DelayTime Values

        //Milli Sec
        private static long[] _enemyDetectedDelayMax = { 0, 3000, 0, 3500, 2000, -1};
        public static long[] enemyDetectedDelayMax { get { return _enemyDetectedDelayMax; } }
        //Milli Sec
        private static long[] _enemyDetectedStayMax = { 0, 1500, 5000, 1500, 0, -1};
        public static long[] enemyDetectedStayMax { get { return _enemyDetectedStayMax; } }
        //Sec
        private static float _deadBodyRemainTime = 30.0f;
        public static float deadBodyRemainTime { get { return _deadBodyRemainTime; } }
        //Sec
        private static int _attackCountDelay = 2;
        public static int attackCountDelay { get { return _attackCountDelay; } } 

        #endregion

        #region Distance Values

        private static float _assassinateAnimDist = 0.86f;
        public static float assassinateAnimDist { get { return _assassinateAnimDist; } }

        private static float _canAssassinateDist = 3.0f;
        public static float canAssassinateDist { get { return _canAssassinateDist; } }

        #endregion
    }

    public static class FilePaths
    {
        private static string _weaponPath = "Weapons/Weapon";
        public static string weaponPath { get { return _weaponPath; } }

        private static string _AISystemPath = "AISystem/";
        public static string AISystemPath { get { return _AISystemPath; } }
    }

    public static class AggroCollections
    {
        private static float _aggroRun = 0.01f;
        private static float _aggroWalk = 0.005f;
        private static float _aggroCrouch = 0.001f;

        private static float _alertMin = 1.0f;
        private static float _combatMin = 2.0f;

        /// <summary>
        /// aggroRun = 0.1f
        /// </summary>
        public static float aggroRun { get { return _aggroRun; } }
        /// <summary>
        /// aggroWalk = 0.05f
        /// </summary>
        public static float aggroWalk { get { return _aggroWalk; } }
        /// <summary>
        /// aggroCrouch = 0.01f
        /// </summary>
        public static float aggroCrouch { get { return _aggroCrouch; } }

        /// <summary>
        /// alertMin = 1.0f
        /// 1.0f 이상이면 curUnitState = ALERT
        /// </summary>
        public static float alertMin { get { return _alertMin; } }
        /// <summary>
        /// combatMin = 2.0f
        /// 2.0f 이상이면 curUnitState = COMBAT
        /// </summary>
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
        private static int _inSmoke = 6;
        //Player만 사용
        private static int _assassinate = 7;

        /// <summary>
        /// Standing = 0
        /// </summary>
        public static int Standing { get { return _standing; } }
        /// <summary>
        /// Crouch = 1
        /// </summary>
        public static int Crouch { get { return _crouch; } }
        /// <summary>
        /// CoverStanding = 2
        /// </summary>
        public static int CoverStanding { get { return _coverStanding; } }
        /// <summary>
        /// CoverCrouch = 3
        /// </summary>
        public static int CoverCrouch { get { return _coverCrouch; } }
        /// <summary>
        /// Throw = 4
        /// </summary>
        public static int Throw { get { return _throw; } }
        /// <summary>
        /// ThrowMove = 5
        /// </summary>
        public static int ThrowMove { get { return _throwMove; } }
        /// <summary>
        /// InSmoke = 6
        /// </summary>
        public static int InSmoke { get { return _inSmoke; } }
        /// <summary>
        /// Assassinate = 7
        /// Player만 사용 가능
        /// </summary>
        public static int Assassinate { get { return _assassinate; } }
    }

    //물리 레이어
    public static class PhysicsLayers
    {
        private static int _Default = 0;
        private static int _TransparentFX = 1;
        private static int _IgnoreRaycast = 2;
        private static int _Water = 4;
        private static int _UI = 5;
        private static int _Lighting = 8;
        private static int _Structure = 9;
        private static int _Enemy = 10;
        private static int _Player = 11;
        private static int _Throw = 12;
        private static int _EnemyAttack = 13;
        private static int _PlayerAttack = 14;
        private static int _EnemyRadar = 15;
        private static int _PlayerRadar = 16;

        /// <summary>
        /// Default = 0
        /// </summary>
        public static int Default { get { return _Default; } }
        /// <summary>
        /// TransparentFX = 1
        /// </summary>
        public static int TransparentFX { get { return _TransparentFX; } }
        /// <summary>
        /// IgnoreRaycast = 2
        /// </summary>
        public static int IgnoreRaycast { get { return _IgnoreRaycast; } }
        /// <summary>
        /// Water = 4
        /// </summary>
        public static int Water { get { return _Water; } }
        /// <summary>
        /// UI = 5
        /// </summary>
        public static int UI { get { return _UI; } }
        /// <summary>
        /// Lighting = 8
        /// </summary>
        public static int Lighting { get { return _Lighting; } }
        /// <summary>
        /// Structure = 9
        /// </summary>
        public static int Structure { get { return _Structure; } }
        /// <summary>
        /// Enemy = 10
        /// </summary>
        public static int Enemy { get { return _Enemy; } }
        /// <summary>
        /// Player = 11
        /// </summary>
        public static int Player { get { return _Player; } }
        /// <summary>
        /// Throw = 12
        /// </summary>
        public static int Throw { get { return _Throw; } }
        /// <summary>
        /// EnemyAttack = 13
        /// </summary>
        public static int EnemyAttack { get { return _EnemyAttack; } }
        /// <summary>
        /// PlayerAttack = 14
        /// </summary>
        public static int PlayerAttack { get { return _PlayerAttack; } }
        /// <summary>
        /// EnemyAttack = 15
        /// </summary>
        public static int EnemyRadar { get { return _EnemyRadar; } }
        /// <summary>
        /// PlayerAttack = 16
        /// </summary>
        public static int PlayerRadar { get { return _PlayerRadar; } }
    }

    //디버그용 
    public static class MyDebug
    {
        public static void Log(object msg)
        {
            UnityEngine.Debug.Log(msg);
        }
    }
}

using System;
using UnityEngine;

using Com.MyCompany.MyGame.FileIO;

//Dic이용해서 정리 한 번 해볼것
namespace Com.MyCompany.MyGame
{
    namespace Collections
    {
        #region Enums
        public enum ItemCode
        {
            GOLD = 0, CAN, CHEESE, SMOKE, max
        }

        //Enemy가 추적할 때 사용하기도 함
        public enum WeaponCode
        {
            HAND = 0, CAN, CHEESE, SMOKE, PLAYER, PLAYERTRACK, ENEMYDEAD, PATROL, max
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
            IDLE = 0, ALERT, COMBAT, CHEESE, INSMOKE, EAT, ASSASSINATE, max
        }
        //캐릭터가 바라보는 방향
        public enum LookDirState
        {
            IDLE = 0, COVER, THROW, FINDPLAYER, AGENT, DIRECT,max
        }

        public enum Score
        {
            ASSASSINATE = 0, NORNALKILL, GETGOLD, max
        }

        #endregion

        #region Classes
        public class UnitStat
        {
            private string _unitCode;

            private float _speed;
            private float _walkVar;
            private float _coverVar;
            private float _health;
            private float _maxHealth;
            private float _jumpPower;

            public string unitCode { get { return _unitCode; } }

            public float speed { get { return _speed; } }
            public float walkSpeed { get { return _walkVar * _speed; } }
            public float coverSpeed { get { return _coverVar * _speed; } }
            public float health { get { return _health; } }
            public float MaxHealth { get { return _maxHealth; } }
            public float jumpPower { get { return _jumpPower; } }

            public float hpRatio { get { return _health / _maxHealth; } }

            public UnitStat(float sp, float walkV, float coverV, float hp, float jump)
            {
                _speed = sp;
                _walkVar = walkV;
                _coverVar = coverV;
                _health = hp;
                _maxHealth = hp;
                _jumpPower = jump;
            }
            public UnitStat(string code,float sp, float walkV, float coverV, float hp, float jump)
            {
                _unitCode = code;
                _speed = sp;
                _walkVar = walkV;
                _coverVar = coverV;
                _health = hp;
                _maxHealth = hp;
                _jumpPower = jump;
            }

            public UnitStat()
            {
                _unitCode = "NULL";
            }
            public UnitStat(string unitCode)
            {
                SetUnitStat(DBIO.Read(unitCode));
            }
            private void SetUnitStat(UnitStat data)
            {
                if (data == null)
                {
                    Debug.LogError("data == NULL, 올바른 unitCode를 사용하십시오");
                    _unitCode = "NULL";
                    _speed = 0;
                    _walkVar = 0;
                    _coverVar = 0;
                    _health = 0;
                    _maxHealth = 0;
                    _jumpPower = 0;
                    return;
                }

                _unitCode = data.unitCode;
                _speed = data.speed;
                _walkVar = data.walkSpeed / _speed;
                _coverVar = data.coverSpeed / _speed;
                _health = data.health;
                _maxHealth = data.MaxHealth;
                _jumpPower = data.jumpPower;
            }

            public void SetHealth(float val)
            {
                _health = val;
            }
            public void SetCode(string code)
            {
                _unitCode = code;
            }
            public void SetSpeed(float val)
            {
                _speed = val;
            }
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
            //public static int CompareEnum<TEnum1, TEnum2>(TEnum1 tEnum1, TEnum2 tEnum2)
            //    where TEnum1 : struct, IConvertible, IComparable, IFormattable
            //    where TEnum2 : struct, IConvertible, IComparable, IFormattable
            //{
            //    return tEnum1.ToString().CompareTo(tEnum2.ToString());
            //}

            public static ItemCode ConvertWeaponToItem(WeaponCode weaponCode)
            {
                ItemCode result = ItemCode.max;
                int index = (int)weaponCode;

                if (index < (int)ItemCode.max) result = (ItemCode)index;

                return result;
            }
        }

        public static class ValueCollections
        {
            private static int[] _itemMaxAmount = { 50, 30, 20 };
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
            private static long[] _enemyDetectedDelayMax = { 0, 3000, 0, 3500, 2000, 0, 3000, 100, -1 };
            public static long[] enemyDetectedDelayMax { get { return _enemyDetectedDelayMax; } }
            //Milli Sec
            private static long[] _enemyDetectedStayMax = { 0, 1500, 5000, 1500, 0, 0, 1000, 100, -1 };
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

            public static Vector3 GetRandomVector3(float dist, int instanceID)
            {
                System.Random random = new System.Random((int)(Time.time * 100 - instanceID * 0.01f));
                int d = random.Next(0, (int)dist);
                Vector3 result = Vector3.zero;
                result.Set((float)random.Next(-d, d), 0, (float)random.Next(-d, d));
                result = result.normalized * d;
                return result;
            }
        }

        public static class FilePaths
        {
            private static string _weaponPath = "Weapons/Weapon";
            public static string weaponPath { get { return _weaponPath; } }

            private static string _AISystemPath = "AISystem/";
            public static string AISystemPath { get { return _AISystemPath; } }

            private static string _AudioPath = "Audios/";
            public static string AudioPath { get { return _AudioPath; } }

            private static string _DataPath = Application.persistentDataPath;
            public static string DataPath { get { return _DataPath; } }

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

        public static class UILayers
        {
            private static int _UI = 5;
            private static int _MiniMap = 21;
            private static int _MiniMap1F = 22;
            private static int _MiniMap2F = 23;
            private static int _MiniMap3F = 24;
            private static int _MiniMap4F = 25;

            /// <summary>
            /// UI = 5
            /// </summary>
            public static int UI { get { return _UI; } }
            /// <summary>
            /// MiniMap1F = 21
            /// </summary>
            public static int MiniMap { get { return _MiniMap; } }
            /// <summary>
            /// MiniMap1F = 22
            /// </summary>
            public static int MiniMap1F { get { return _MiniMap1F; } }
            /// <summary>
            /// MiniMap2F = 23
            /// </summary>
            public static int MiniMap2F { get { return _MiniMap2F; } }
            /// <summary>
            /// MiniMap3F = 24
            /// </summary>
            public static int MiniMap3F { get { return _MiniMap3F; } }
            /// <summary>
            /// MiniMap4F = 25
            /// </summary>
            public static int MiniMap4F { get { return _MiniMap4F; } }
        }

        //물리 레이어
        public static class PhysicsLayers
        {
            private static int _Default = 0;
            private static int _TransparentFX = 1;
            private static int _IgnoreRaycast = 2;
            private static int _Water = 4;
            private static int _Lighting = 8;
            private static int _Structure = 9;
            private static int _Enemy = 10;
            private static int _Player = 11;
            private static int _Throw = 12;
            private static int _EnemyAttack = 13;
            private static int _PlayerAttack = 14;
            private static int _EnemyRadar = 15;
            private static int _PlayerRadar = 16;
            private static int _TargetLayer = 17;
            private static int _EnemyRadarManager = 18;
            private static int _Weapon = 19;
            private static int _Item = 20;

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
            /// <summary>
            /// TargetLayer = 17
            /// </summary>
            public static int TargetLayer { get { return _TargetLayer; } }
            /// <summary>
            /// EnemyRadarManager = 18
            /// </summary>
            public static int EnemyRadarManager { get { return _EnemyRadarManager; } }
            /// <summary>
            /// Weapon = 19
            /// </summary>
            public static int Weapon { get { return _Weapon; } }
            /// <summary>
            /// Item = 20
            /// </summary>
            public static int Item { get { return _Item; } }
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

    namespace Exceptions
    {
        [Serializable]  public class AIsCloseToB : System.Exception
        {
            public AIsCloseToB(){}
        }
        [Serializable]  public class FreezingUnitException : System.Exception
        {
            public FreezingUnitException() { }
        }
        [Serializable]  public class AnimationIsPlayingException : System.Exception
        {
            public AnimationIsPlayingException() { }
        }

        public static class ValidateException
        {
            public static void CheckAIsCloseToB(Vector3 a, Vector3 b, float dist)
            {
                if (Vector3.Distance(a, b) <= dist) throw new AIsCloseToB();
            }
            public static void ValidateFreezingUnitException(bool boolVal, AnimatorStateInfo animInfo, string animName)
            {
                if (animInfo.IsName(animName) && boolVal) throw new FreezingUnitException();
            }
            public static void ValidateFreezingUnitAttackException(bool isLocked, AnimatorStateInfo animInfo, string animName, bool isAttack)
            {
                if (!animInfo.IsTag("Attack") && ((isLocked && !animInfo.IsName(animName) || isAttack))) throw new FreezingUnitException();
            }
            public static void ValidateAnimationIsPlayingException(AnimatorStateInfo animInfo, string animName)
            {
                if (animInfo.IsName(animName))
                    throw new AnimationIsPlayingException();
            }
            public static void ValidateAnimationIsPlayingException(AnimatorStateInfo animInfo, string animName, bool animBool)
            {
                if (!animInfo.IsName(animName) && animBool)
                    throw new AnimationIsPlayingException();
            }
        }
    }
}

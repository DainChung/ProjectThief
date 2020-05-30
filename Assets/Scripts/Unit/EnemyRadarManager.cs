using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.Exceptions;

namespace Com.MyCompany.MyGame
{
    public class EnemyRadarManager : MonoBehaviour
    {
        private Unit unit;
        private MeshCollider eyeCollider;

        public EnemyController enemy;
        public EnemyRadar eye;

        void Start()
        {
            unit = enemy.transform.GetComponent<Unit>();
            eyeCollider = eye.GetComponent<MeshCollider>();
        }

        void OnTriggerStay(Collider other)
        {
            //암살당할때는 작동 안 함
            if (!enemy.assassinateTargetted)
            {
                int layer = other.gameObject.layer;
                #region 연막탄인 경우
                if (layer == PhysicsLayers.TargetLayer)
                {
                    try
                    {
                        switch (other.GetComponent<Target>().code)
                        {
                            case WeaponCode.SMOKE:
                                unit.curLookDir = LookDirState.DIRECT;
                                enemy.lookDir = 2 * enemy.transform.position - other.transform.position;
                                eyeCollider.enabled = false;
                                break;
                            default:
                                break;

                        }
                    }
                    catch (System.Exception) { }
                }
                #endregion

                #region Player인 경우
                if (other.gameObject.layer == PhysicsLayers.Player)
                {
                    try
                    {
                        ValidateException.CheckAIsCloseToB(enemy.transform.position, other.transform.position, 1.2f);
                        enemy.doesReachToTarget = false;
                    }
                    catch (AIsCloseToB)
                    {
                        enemy.doesReachToTarget = true;
                    }

                    switch (unit.curUnitState)
                    {
                        case UnitState.CHEESE:
                            break;
                        case UnitState.IDLE:
                            eye.PlayerDetection(other.transform);
                            break;
                        default:
                            enemy.IsMovingNow = false;
                            unit.curUnitPose = UnitPose.MOD_ATTACK;
                            if (unit.alertValue < AggroCollections.combatMin)
                            {
                                unit.alertValue = AggroCollections.combatMin;
                                unit.AlertManager();
                            }
                            break;
                    }
                }
                #endregion

                #region Cheese인 경우
                if (layer == PhysicsLayers.Item && other.name.Contains("CHEESE"))
                    other.GetComponent<WeaponThrow>().PoolAggro();
                #endregion
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Player)
            {
                enemy.doesReachToTarget = false;
                enemy.CanIAttack = false;
                try{ eyeCollider.enabled = true; }
                catch (System.Exception) { }
            }
        }    
    }
}

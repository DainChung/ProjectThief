using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyRadarManager : MonoBehaviour
    {
        private Unit unit;

        public EnemyController enemy;
        public EnemyRadar enemyRadar;

        void Start()
        {
            unit = enemy.transform.GetComponent<Unit>();
        }

        void OnTriggerStay(Collider other)
        {
            //암살당할때는 작동 안 함
            if (!enemy.assassinateTargetted)
            {
                #region 연막탄인 경우
                if (other.gameObject.layer == PhysicsLayers.TargetLayer)
                {
                    switch (other.GetComponent<Target>().code)
                    {
                        case WeaponCode.SMOKE:
                            unit.curLookDir = LookDirState.DIRECT;
                            enemy.lookDir = 2 * enemy.transform.position - other.transform.position;
                            enemy.transform.GetChild(0).Find("EnemyRadarEye").GetComponent<MeshCollider>().enabled = true;
                            break;
                        default:
                            break;

                    }
                }
                #endregion

                #region Player인 경우
                if (other.CompareTag("Player"))
                {
                    switch (unit.curUnitState)
                    {
                        case UnitState.CHEESE:
                            break;
                        case UnitState.IDLE:
                            enemyRadar.PlayerDetection(other.transform);
                            break;
                        default:
                            enemy.doesReachToTarget = true;
                            enemy.IsMovingNow = false;
                            unit.curUnitPose = UnitPose.MOD_ATTACK;
                            if (unit.alertValue < AggroCollections.combatMin)
                            {
                                unit.alertValue = AggroCollections.combatMin;
                                unit.AlertManager();
                            }

                            //int index = 0;

                            //while (index < other.transform.childCount)
                            //{
                            //    if (other.transform.GetChild(index).name == "Target(Clone)")
                            //    {
                            //        try
                            //        {
                            //            if (enemy.transform.GetInstanceID() == other.transform.GetChild(index).GetComponent<Target>().ID)
                            //                Destroy(other.transform.GetChild(index).gameObject);
                            //            index++;
                            //        }
                            //        catch (System.Exception) { }
                            //    }
                            //}
                            break;
                    }
                }
                #endregion
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                enemy.doesReachToTarget = false;
                enemy.CanIAttack = false;
            }
        }    
    }
}

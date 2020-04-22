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

                            int index = ValueCollections.TargetChildIndex;

                            while (index < other.transform.childCount)
                            {
                                try
                                {
                                    if (enemy.transform.GetInstanceID() == other.transform.GetChild(index).GetComponent<Target>().ID)
                                        Destroy(other.transform.GetChild(index).gameObject);
                                    index++;
                                }
                                catch (System.Exception)
                                { }
                            }
                            break;
                    }
                }
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

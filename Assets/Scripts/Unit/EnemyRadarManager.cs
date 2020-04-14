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

        void Start()
        {
            unit = enemy.transform.GetComponent<Unit>();
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && unit.curUnitState != UnitState.CHEESE)
            {
                if (enemy.IsAlertValueSmallerThen(AggroCollections.combatMin))
                    enemy.SetAlertValue(AggroCollections.combatMin);
                enemy.doesReachToTarget = true;
                enemy.IsMovingNow = false;
                unit.curUnitPose = UnitPose.MOD_ATTACK;
                if(unit.alertValue < AggroCollections.combatMin)
                    unit.alertValue = AggroCollections.combatMin;

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

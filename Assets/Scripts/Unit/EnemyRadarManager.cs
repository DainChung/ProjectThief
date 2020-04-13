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

        public IEnumerator CheckToTarget(Vector3 pos)
        {
            //if (pos == ValueCollections.initPos)
            //    yield break;

            //Ray ray = new Ray();
            //Vector3 h = new Vector3(-0.5f, 0.55f, 0);

            //ray.origin = enemy.transform.position + h;
            //ray.direction = pos - enemy.transform.position + h;

            //float dist = Vector3.Distance(pos, transform.position);
            //RaycastHit[] hits = Physics.SphereCastAll(ray, 0.5f, dist);

            //Debug.DrawRay(ray.origin, ray.direction, Color.red, 10);
            //foreach (RaycastHit hit in hits)
            //{
            //    try
            //    {
            //        //사이에 장애물 있으면 agent.SetDestination()으로 장애물을 우회하도록 한다.
            //        if (hit.transform.gameObject.layer == PhysicsLayers.Structure)
            //        {
                        
            //            //코루틴 종료
            //            yield break;
            //        }
            //        else if (hit.transform.gameObject.layer != PhysicsLayers.Default)
            //            _thereIsStructure = false;
            //    }
            //    catch (System.Exception)
            //    {}

            //    yield return null;
            //}

            yield break;
        }      
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyRadarManager : MonoBehaviour
    {
        private bool _thereIsStructureR = false;
        private bool _thereIsStructureL = false;
        private Unit unit;

        public EnemyController enemy;
        public bool thereIsStructure { get { return (_thereIsStructureR && _thereIsStructureL); } }

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

        private IEnumerator CheckToTarget(Vector3 pos, bool isRight)
        {
            Ray ray = new Ray();

            if (isRight)     ray.origin = enemy.transform.position + transform.right;
            else             ray.origin = enemy.transform.position - transform.right;

            ray.direction = pos - enemy.transform.position;
            float dist = Vector3.Distance(pos, transform.position) + 3f;
            RaycastHit[] hits = Physics.RaycastAll(ray, dist);
            Debug.DrawRay(ray.origin, ray.direction * dist, Color.red, 10.0f);

            foreach (RaycastHit hit in hits)
            {
                try
                {
                    //사이에 장애물 있으면 agent.SetDestination()으로 장애물을 우회하도록 한다.
                    if (hit.transform.gameObject.layer == PhysicsLayers.Structure)
                    {
                        if (isRight)    _thereIsStructureR = true;
                        else            _thereIsStructureL = true;

                        //코루틴 종료
                        yield break;
                    }
                }
                catch (System.Exception)
                {
                    yield break;
                }

                yield return null;
            }

            //사이에 장애물이 없으면 rigibBody.AddForce()를 사용해서 이동한다.
            if (isRight)    _thereIsStructureR = false;
            else            _thereIsStructureL = false;

            yield break;
        }
        public IEnumerator CheckToTarget(Vector3 pos)
        {
            StartCoroutine(CheckToTarget(pos, true));
            StartCoroutine(CheckToTarget(pos, false));

            yield break;
        }      
    }
}

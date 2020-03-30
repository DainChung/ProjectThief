using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyRadar : MonoBehaviour
    {
        //private Vector3 rayOrigin = new Vector3;
        private Vector3 height = new Vector3(0, 0.1f, 0);

        public Unit unit;
        public float radarValue;

        public Transform eyes;

        void FixedUpdate()
        {
            if(eyes != null)
                transform.rotation = Quaternion.Euler(-90f, eyes.rotation.eulerAngles.y, 0);
        }

        void OnTriggerStay(Collider other)
        {
            if (unit.health <= 0)
                Destroy(gameObject);

            if (other.transform.gameObject.layer == PhysicsLayers.Player)
            {
                RaycastHit hit;
                Vector3 rayOrigin = unit.transform.position - height;
                Vector3 rayDesti = other.transform.position - rayOrigin;

                Debug.DrawRay(rayOrigin, rayDesti, Color.white, 1.0f);
                if (Physics.Raycast(rayOrigin, rayDesti, out hit) && (hit.transform.gameObject.layer == PhysicsLayers.Player))
                {
                    unit.AddToAlertValue(other.transform.GetComponent<PlayerController>().aggroVal * radarValue);

                    if (unit.alertValue > 0.5f)
                        unit.transform.LookAt(other.transform.position);

                    if (Vector3.Distance(unit.transform.position, other.transform.position) > 1.2f)
                    {
                        if (unit.curUnitState == UnitState.ALERT)
                            unit.transform.GetComponent<EnemyController>().Detect(WeaponCode.PLAYERTRACK, other.transform.position);
                        //다른 방법으로 Player를 추적하도록 해볼것
                        else if (unit.curUnitState == UnitState.COMBAT)
                            unit.transform.GetComponent<EnemyController>().Detect(WeaponCode.PLAYER, other.transform.position);
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (unit.health <= 0)
                Destroy(gameObject);

            if (other.transform.gameObject.layer == PhysicsLayers.Player)
                StartCoroutine(DecreaseAlertValue());
        }

        private IEnumerator DecreaseAlertValue()
        {
            Debug.Log("놓침");
            //전투 중이면 일정 시간 대기 후 경계상태로 전환
            if (unit.alertValue >= AggroCollections.combatMin)
            {
                Debug.Log("대기 시작");
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds <= 4000)
                    yield return null;

                sw.Stop();

                unit.SetAlertValue(AggroCollections.combatMin - 0.1f);
                Debug.Log("경계로 전환");
                yield break;
            }
            //비전투 상태면
            else
            {
                //일정 시간 대기 후
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds <= 2000)
                    yield return null;

                sw.Stop();

                //서서히 어그로 감소
                while (unit.alertValue >= AggroCollections.alertMin)
                {
                    unit.AddToAlertValue(-0.01f);
                    yield return null;
                }

                Debug.Log("평시상태로 전환");
            }

            yield break;
        }
    }
}
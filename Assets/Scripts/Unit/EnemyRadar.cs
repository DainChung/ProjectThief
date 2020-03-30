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

        void OnTriggerStay(Collider other)
        {
            if (other.transform.gameObject.layer == PhysicsLayers.Player)
            {
                RaycastHit hit;
                Vector3 rayOrigin = unit.transform.position - height;
                Vector3 rayDesti = other.transform.position - rayOrigin;

                //Debug.DrawRay(rayOrigin, rayDesti, Color.white, 1.0f);
                if (Physics.Raycast(rayOrigin, rayDesti, out hit) && (hit.transform.gameObject.layer == PhysicsLayers.Player))
                    unit.AddToAlertValue(other.transform.GetComponent<PlayerController>().aggroVal * radarValue);

                //Debug.Log(LayerMask.LayerToName(hit.transform.gameObject.layer));
            }
        }

        void OnTriggerExit(Collider other)
        {
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

                while (sw.ElapsedMilliseconds <= 1000)
                    yield return null;

                sw.Stop();

                unit.SetAlertValue(AggroCollections.combatMin - 0.1f);
                Debug.Log("경계로 전환");
                yield break;
            }
            //비전투 상태면
            else
            {
                //서서히 어그로 감소
                while (unit.alertValue >= AggroCollections.alertMin)
                {
                    Debug.Log(unit.alertValue);
                    unit.AddToAlertValue(-0.01f);
                    yield return null;
                }

                Debug.Log("평시상태로 전환");
            }

            yield break;
        }
    }
}
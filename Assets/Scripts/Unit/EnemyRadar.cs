using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyRadar : MonoBehaviour
    {
        private Vector3 height = new Vector3(0, 0.1f, 0);
        private bool exitCoroutine = false;
        private Unit unit;

        public float radarValue;
        [HideInInspector] public Stopwatch delays = new Stopwatch();

        public Transform eyes;
        public EnemyCheckStructure checkStructure;

        void Start()
        {
            unit = transform.parent.parent.GetComponent<Unit>();
        }

        void FixedUpdate()
        {
            transform.rotation = Quaternion.Euler(-90f, eyes.rotation.eulerAngles.y, 0);
            if (delays.Elapsed.Seconds > 10) StartCoroutine(DecreaseAlertValue());
        }

        void OnTriggerStay(Collider other)
        {
            if (unit.health <= 0)
                Destroy(gameObject);

            if (other.transform.gameObject.layer == PhysicsLayers.Player && unit.curUnitState != UnitState.CHEESE)
                PlayerDetection(other.transform);
        }
        void OnTriggerExit(Collider other)
        {
            if (unit.health <= 0)
                Destroy(gameObject);

            if (other.transform.gameObject.layer == PhysicsLayers.Player)
            {
                exitCoroutine = false;
                StartCoroutine(DecreaseAlertValue());
                delays.Start();
            }

            if (other.gameObject.layer == PhysicsLayers.Weapon)
            {
                unit.curUnitState = UnitState.ALERT;
                unit.transform.GetComponent<EnemyController>().lookDir = other.transform.position;
                unit.transform.GetComponent<EnemyController>().Stop();
                delays.Start();
            }
        }

        public void PlayerDetection(Transform otherTR)
        {
            Vector3 otherPos = otherTR.position;
            RaycastHit[] hits;
            Vector3 rayOrigin = unit.transform.position + height;
            Vector3 rayDesti = otherPos - rayOrigin;

            hits = Physics.RaycastAll(rayOrigin, rayDesti);
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.layer == PhysicsLayers.Structure)
                    break;

                if (hit.transform.gameObject.layer == PhysicsLayers.Player)
                {
                    try
                    {
                        checkStructure.CheckStructure(hit.transform.position);
                        if (unit.curUnitState != UnitState.INSMOKE && !checkStructure.isThereStructure)
                        {
                            if (unit.alertValue > 0.15f) unit.curLookDir = LookDirState.FINDPLAYER;
                            //플레이어가 인지 범위 밖으로 갔다가 다시 들어오면 DecreaseAlertValue 코루틴을 정지한다.
                            exitCoroutine = true;

                            float distVal = Mathf.Clamp(Vector3.Distance(otherPos, unit.transform.position), 0.1f, 7);
                            unit.AddToAlertValue(otherTR.GetComponent<PlayerController>().aggroVal * radarValue / distVal);

                            if (unit.curUnitState == UnitState.ALERT) unit.transform.GetComponent<EnemyController>().Detect(WeaponCode.PLAYERTRACK, otherTR, otherPos);
                            else if (unit.curUnitState == UnitState.COMBAT) unit.transform.GetComponent<EnemyController>().Detect(WeaponCode.PLAYER, otherTR, otherPos);
                        }
                    }
                    catch (System.Exception) { }
                }
            }
        }

        private IEnumerator DecreaseAlertValue()
        {
            //전투 중이면 일정 시간 대기 후 경계상태로 전환
            if (unit.alertValue >= AggroCollections.combatMin)
            {
                delays.Reset();
                delays.Stop();
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds <= 10000)
                {
                    if (exitCoroutine)
                        yield break;
                    yield return null;
                }

                sw.Stop();

                unit.SetAlertValue(AggroCollections.combatMin - 0.1f);
                unit.curLookDir = LookDirState.IDLE;
                yield break;
            }
            //비전투 상태면
            else
            {
                delays.Reset();
                delays.Stop();
                //일정 시간 대기 후
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds <= 2000)
                {
                    if (exitCoroutine)
                        yield break;
                    yield return null;
                }

                sw.Stop();

                //서서히 어그로 감소
                while (unit.alertValue >= AggroCollections.alertMin)
                {
                    unit.AddToAlertValue(-0.01f);
                    yield return null;
                }

                unit.curUnitState = UnitState.IDLE;
                unit.curLookDir = LookDirState.IDLE;
            }

            yield break;
        }
    }
}
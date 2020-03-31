using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyRadarManager : MonoBehaviour
    {
        private Vector3 heightRight = new Vector3(0.1f, 0.2f, 0);

        public EnemyController enemy;
        public EnemyRadar[] enemyRadars = new EnemyRadar[3];

        void Start()
        {
            enemyRadars[0].enabled = true;
            enemyRadars[1].enabled = true;
            enemyRadars[2].enabled = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EnableEnemyRadar(false);

                if(enemy.IsAlertValueSmallerThen(AggroCollections.combatMin))
                    enemy.SetAlertValue(AggroCollections.combatMin);
                enemy.doesReachToTarget = true;
                Debug.Log("EnemyradarController.OnTriggerEnter: " + true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EnableEnemyRadar(true);

                enemy.doesReachToTarget = false;
                Debug.Log("EnemyradarController.OnTriggerExit: " + false);
            }
        }

        public void EnableEnemyRadar(bool enable)
        {
            enemyRadars[0].enabled = enable;
            enemyRadars[1].enabled = enable;
            enemyRadars[2].enabled = enable;
        }

        public IEnumerator Sample(Vector3 pos)
        {
            RaycastHit[] hits = Physics.RaycastAll(enemy.transform.position + heightRight, pos - enemy.transform.position);

            Debug.DrawRay(enemy.transform.position + heightRight, pos - enemy.transform.position, Color.red, 1.0f);
            foreach (RaycastHit hit in hits)
            {
                Debug.Log("Hit Info: " + LayerMask.LayerToName(hit.transform.gameObject.layer) + ", " + hit.transform.position);
            }

            yield break;
        }
    }
}

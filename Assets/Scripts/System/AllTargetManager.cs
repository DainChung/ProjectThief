using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class AllTargetManager : MonoBehaviour
    {
        private GameObject[] targets = new GameObject[10];
        private int countOFEnemy = 0;

        public void FindAllTargets()
        {
            countOFEnemy = GameObject.FindGameObjectsWithTag("Enemy").Length;
            targets = GameObject.FindGameObjectsWithTag("Target");
            if (targets.Length > countOFEnemy * 5)
            {
                for (int i = 0; i < targets.Length; i++)
                    Destroy(targets[i]);
            }
        }
    }
}
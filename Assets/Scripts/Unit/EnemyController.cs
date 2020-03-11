using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class EnemyController : MonoBehaviour
    {
        #region Sub Struct

        struct DetectedWeapon
        {
            public WeaponCode code;
            public Vector3 pos;
        }

        #endregion

        #region Sub Classes

        private class DetectedWeaponQueue
        {
            //어그로 끈 Weapon을 저장하는 스택, 최대 3개까지만 기억
            //4번째 입력 OR 치즈가 감지된 경우 초기화 후 재입력
            private Queue<DetectedWeapon> queue = new Queue<DetectedWeapon>();

            public int Count { get { return queue.Count; } }

            public void Enqueue(WeaponCode weaponCode, Vector3 position)
            {
                DetectedWeapon input;
                input.code = weaponCode;
                input.pos = position;

                //중복 내용이 없을 때 입력
                if (!queue.Contains(input))
                {
                    //4번째 입력 시 초기화 OR 치즈인 경우 초기화
                    if (queue.Count >= 3 || weaponCode == WeaponCode.CHEESE)
                        queue.Clear();

                    queue.Enqueue(input);

                    UnityEngine.Debug.Log("Enqueue " + queue.Count + " : " + input.code + ", " + input.pos);
                }
            }

            public DetectedWeapon Dequeue()
            {
                DetectedWeapon result;

                if (queue.Count > 0)
                    result = queue.Dequeue();
                else
                {
                    result.code = WeaponCode.max;
                    result.pos = new Vector3(-1,-1,-1);
                }

                return result;
            }
        }

        #endregion

        #region Private Fields

        private DetectedWeaponQueue queue = new DetectedWeaponQueue();

        private LookDirState curLookDir;
        private UnitState curState;

        private Unit unit;

        #endregion

        #region Public Fields

        public Transform throwPos;

        #endregion


        #region MonoBehaviour Callbacks

        void Awake()
        {
            curLookDir = LookDirState.IDLE;
            curState = UnitState.IDLE;
        }

        void Start()
        {
            unit = GetComponent<Unit>();
        }


        void FixedUpdate()
        {
            //if (queue.Count >= 3)
            //{
            //    DetectedWeapon w = queue.Dequeue();
            //    UnityEngine.Debug.Log("1 Dequeue: " + w.code + ", " + w.pos);
            //    w = queue.Dequeue();
            //    UnityEngine.Debug.Log("2 Dequeue: " + w.code + ", " + w.pos);
            //    w = queue.Dequeue();
            //    UnityEngine.Debug.Log("3 Dequeue: " + w.code + ", " + w.pos);
            //}
        }
        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public void DetectWeapon(WeaponCode code, Vector3 pos)
        {
            curState = UnitState.ALERT;

            //UnityEngine.Debug.Log("Detect: " + code + ", Pos: " + pos + ", State: " + curState);

            queue.Enqueue(code, pos);
        }


        //미정
        public void DetectPlayer(Vector3 pos, ref UnitState curState)
        {
            curState = UnitState.COMBAT;
        }

        #endregion

    }
}

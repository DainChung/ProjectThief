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

        //우선순위 : 치즈 > 캔 = 연막
        private class DetectedWeaponQueue
        {
            //어그로 끈 Weapon을 저장하는 배열, 최대 3개까지만 기억
            private DetectedWeapon[] queue = new DetectedWeapon[3];

            public DetectedWeaponQueue()
            {
                DetectedWeapon init;
                init.code = WeaponCode.max;
                init.pos = new Vector3(-1,-1,-1);

                queue[0] = init;
                queue[1] = init;
                queue[2] = init;
            }

            public void Enqueue(WeaponCode weaponCode, Vector3 position)
            {
                DetectedWeapon input;
                input.code = weaponCode;
                input.pos = position;

                //중복된 내용 아님
                if (!IsItDuplicate(input))
                {
                    if (Count() <= 2)
                    {
                        if (weaponCode != WeaponCode.CHEESE && weaponCode != WeaponCode.max)
                        {
                            queue[Count()] = input;
                        }
                        else if (weaponCode == WeaponCode.CHEESE)
                        {
                            //치즈면 최우선순위로 설정
                            PushQueueReverse();
                            queue[0] = input;
                        }
                    }
                }
            }

            //queue[0]부터 차례대로 리턴
            public DetectedWeapon Dequeue()
            {
                DetectedWeapon result;

                if (Count() > 0)
                {
                    result = queue[0];
                    PushQueue();
                }
                else
                {
                    result.code = WeaponCode.max;
                    result.pos = new Vector3(-1, -1, -1);
                }

                return result;
            }

            public int Count()
            {
                int result = 0;

                for (int i = 0; i < queue.Length; i++)
                {
                    if (queue[i].code != WeaponCode.max)
                        result = i+1;
                }

                return result;
            }

            //중복된 내용이면 true, 아니면 false
            private bool IsItDuplicate(DetectedWeapon input)
            {
                for (int i = 0; i < queue.Length; i++)
                {
                    if ((queue[i].code == input.code) && (queue[i].pos == input.pos))
                        return true;
                }

                return false;
            }

            private void PushQueue()
            {
                for (int i = 1; i < queue.Length; i++)
                {
                    queue[i - 1] = queue[i];
                    queue[i].code = WeaponCode.max;
                    queue[i].pos.Set(-1,-1,-1);
                }
            }
            private void PushQueueReverse()
            {
                for (int i = queue.Length - 2; i >= 0; i--)
                {
                    queue[i + 1] = queue[i];
                    queue[i].code = WeaponCode.max;
                    queue[i].pos.Set(-1, -1, -1);
                }
            }
        }

        #endregion

        #region Private Fields

        private DetectedWeaponQueue queue = new DetectedWeaponQueue();

        private LookDirState curLookDir;
        private UnitState curState;

        private Unit unit;

        private float alert = 0.0f;

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

        }
        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public void DetectWeapon(WeaponCode code, Vector3 pos)
        {
            curState = UnitState.ALERT;

            alert = AggroCollections.alertMin;
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

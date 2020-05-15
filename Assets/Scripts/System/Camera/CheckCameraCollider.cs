using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class CheckCameraCollider : MonoBehaviour
    {
        #region Private Fields
        private Camera mainCam;

        private bool _canAssassinate = false;
        private Transform _assassinateTarget = null;
        private EnemyController _assassinateEnemy = null;

        private Vector3 height = new Vector3(0, 1, 0);
        private Vector3 rayOrigin;
        private Vector3 rayDesti;

        private PlayerController playerController;
        private Unit unit;

        private bool check = false;

        #endregion

        #region Public Fields

        public bool canAssassinate { get { return (_assassinateTarget == null ? false : _canAssassinate); } }
        public EnemyController assassinateEnemy { get { return ((_assassinateTarget == null) ? null : _assassinateEnemy); } }
        public Vector3 assassinateTargetPos { get { return ((_assassinateTarget == null) ? ValueCollections.initPos : _assassinateTarget.position); } }

        #endregion

        #region MonoBehaviour Callbacks
        void Start()
        {
            playerController = transform.parent.GetComponent<PlayerController>();
            unit = transform.parent.GetComponent<Unit>();
            mainCam = Camera.main;
        }
        void FixedUpdate()
        {
            //메인카메라 따라서 회전
            transform.rotation = Quaternion.Euler(-90f, mainCam.transform.rotation.eulerAngles.y, 0);

            //땅에 붙어있을 때만 작동
            if(unit.IsOnFloor() && check)
                CheckEnemies();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
            {
                check = true;
                other.transform.GetComponent<EnemyController>().canAssassinate = true;
            }
        }
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
            {
                check = true;
                other.transform.GetComponent<EnemyController>().canAssassinate = true;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
            {
                check = false;
                other.transform.GetComponent<EnemyController>().canAssassinate = false;
                playerController.SetIndicator("AssassinateIndicator", null);
            }
        }
        #endregion

        #region Private Methods

        private void CheckEnemies()
        {
            rayOrigin = transform.parent.position + height;
            RaycastHit[] hits = Physics.SphereCastAll(rayOrigin, ValueCollections.canAssassinateDist, -transform.up, 0.1f, 1 << PhysicsLayers.Enemy);
            Debug.DrawRay(rayOrigin, -transform.up * 0.1f, Color.red);

            //가까운 거리에 Enemy 없음
            if (hits.Length == 0)
            {
                _assassinateTarget = null;
                _canAssassinate = false;
                check = false;
            }
            else
            {
                foreach (RaycastHit obj in hits)
                {
                    RaycastHit hit = new RaycastHit();
                    rayDesti = -rayOrigin + obj.transform.position;

                    //Enemy와 Player 사이에 장애물이 있으면 암살 불가능
                    Debug.DrawRay(rayOrigin, rayDesti, Color.red);
                    if (Physics.Raycast(rayOrigin, rayDesti, out hit) && hit.transform.gameObject.layer == PhysicsLayers.Structure)
                    {
                        if (obj.transform.GetComponent<EnemyController>().canAssassinate) playerController.SetIndicator("AssassinateIndicator", null);
                        _canAssassinate = false;
                        InitAssassinateTargetPos();
                        continue;
                    }

                    if (obj.transform.GetComponent<EnemyController>().canAssassinate)
                    {
                        if (obj.transform.GetComponent<Unit>().health > 0 && unit.health > 0) playerController.SetIndicator("AssassinateIndicator", obj.transform);
                        _canAssassinate = true;
                        _assassinateTarget = obj.transform;
                        _assassinateEnemy = _assassinateTarget.GetComponent<EnemyController>();
                        break;
                    }
                    else _assassinateTarget = null;
                }
            }
        }

        #endregion

        #region Public Methods

        public void InitAssassinateTargetPos()
        {
            _assassinateTarget = null;
        }

        public void InitCanAssassinate()
        {
            _canAssassinate = false;
        }

        #endregion
    }
}

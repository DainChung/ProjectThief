﻿using System.Collections;
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
        private Vector3 _assassinateTargetPos = ValueCollections.initPos;

        private Vector3 height = new Vector3(0, 0.1f, 0);
        private Vector3 rayOrigin;

        #endregion

        #region Public Fields

        public bool canAssassinate { get { return _canAssassinate; } }
        public Vector3 assassinateTargetPos { get { return _assassinateTargetPos; } }

        #endregion

        #region MonoBehaviour Callbacks
        void Start()
        {
            mainCam = Camera.main;
        }

        void FixedUpdate()
        {
            //메인카메라 따라서 회전
            transform.rotation = Quaternion.Euler(-90f, mainCam.transform.rotation.eulerAngles.y, 0);

            CheckEnemies();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
                other.GetComponent<EnemyController>().seenByCamera = true;
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
                other.GetComponent<EnemyController>().seenByCamera = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsLayers.Enemy)
                other.GetComponent<EnemyController>().seenByCamera = false;
        }
        #endregion


        #region Private Methods

        private void CheckEnemies()
        {
            rayOrigin = transform.position + height;
            RaycastHit[] hits = Physics.SphereCastAll(rayOrigin, ValueCollections.canAssassinateDist, -transform.up, 0.1f, 1 << PhysicsLayers.Enemy);

            foreach (RaycastHit obj in hits)
            {
                RaycastHit hit = new RaycastHit();

                if (obj.transform.GetComponent<EnemyController>().seenByCamera)
                {
                    _canAssassinate = true;
                    _assassinateTargetPos = obj.transform.position;
                }

                Debug.DrawRay(rayOrigin, -transform.up * 3, Color.white, 1.0f);
                //Enemy와 Player 사이에 장애물이 있으면 암살 불가능
                if (Physics.Raycast(rayOrigin, -transform.up * ValueCollections.canAssassinateDist, out hit))
                {
                    Debug.Log(LayerMask.LayerToName(hit.transform.gameObject.layer));

                    if (hit.transform.gameObject.layer == PhysicsLayers.Structure)
                    {
                        _canAssassinate = false;
                        InitAssassinateTargetPos();
                    }
                }
            }

            //가까운 거리에 Enemy 없음
            if (hits.Length == 0)
            {
                _assassinateTargetPos = ValueCollections.initPos;
                _canAssassinate = false;
            }

            if (_canAssassinate)
                MyDebug.Log("암살 가능");
        }

        #endregion

        #region Public Methods

        public void InitAssassinateTargetPos()
        {
            _assassinateTargetPos = ValueCollections.initPos;
        }

        public void InitCanAssassinate()
        {
            _canAssassinate = false;
        }

        #endregion
    }
}

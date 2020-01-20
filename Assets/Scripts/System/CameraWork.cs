using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CameraWork : MonoBehaviour
    {
        #region Public Var

        public Vector3 dist;
        public float smooth;

        #endregion

        #region Private Var

        private Transform player;
        private Vector3 playerPos;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Update is called once per frame
        void Update()
        {
            FollowPlayer();
        }

        #endregion

        #region Private Methods

        private void FollowPlayer()
        {
            transform.position = Vector3.Lerp(transform.position, player.position + dist, Time.deltaTime * smooth * player.GetComponent<Unit>().speed);
        }

        #endregion
    }
}

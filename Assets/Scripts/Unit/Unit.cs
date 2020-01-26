using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{

    public class Unit : MonoBehaviour
    {
        #region Private Values

        private float _speed;
        private uint _health;
        private bool _isOnFloor = false;

        #endregion

        #region Public Values

        public float speed { get { return _speed; } set { _speed = value; } }
        public uint health { get { return _health; } }
        public bool isOnFloor { get { return _isOnFloor; } }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            _health = 1;
            _speed = 50.0f;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerStay(Collider other)
        {
            if (other.tag.CompareTo("Floor") == 0)
                _isOnFloor = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.tag.CompareTo("Floor") == 0)
                _isOnFloor = false;
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        public void HitHealth(uint damage)
        {
            if (damage >= _health)
                _health = 0;
            else
                _health -= damage;
        }

        #endregion
    }
}

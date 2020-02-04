using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class Unit : MonoBehaviour
    {
        #region Private Fields

        private float _speed;
        private uint _health;

        #endregion

        #region Public Fields

        public float speed { get { return _speed; } set { _speed = value; } }
        public float walkSpeed { get { return 0.4f * _speed; } }
        public uint health { get { return _health; } }

        #endregion

        #region MonoBehaviour CallBacks
        void Awake()
        {
            _health = 1;
            _speed = 40.0f;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Public Methods

        public void HitHealth(uint damage)
        {
            if (damage >= _health)
                _health = 0;
            else
                _health -= damage;
        }

        public void SetCoverPosition(Vector3 wallPosition, Vector3 wallRight)
        {
            float newX, newZ;
            float alpha = wallRight.z / wallRight.x;
            newZ = wallPosition.z + alpha * (transform.position.x - wallPosition.x);
            newX = wallPosition.x + (newZ - wallPosition.z) / alpha;

            transform.position = new Vector3(newX, transform.position.y, newZ);
        }

        #endregion
    }
}

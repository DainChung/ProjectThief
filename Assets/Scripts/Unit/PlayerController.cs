using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerController : MonoBehaviour
    {
        #region Private Vars

        private float playerSpeed;

        private Transform mainCameraTransform;
        private Quaternion destiRotation;
        private Vector3 lookDir;

        #endregion

        #region Public Vars
        [HideInInspector]
        public Unit unit;

        [HideInInspector]
        public Rigidbody rigidBody;
        #endregion

        #region MonoBehaviour Callback

        void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            unit = gameObject.GetComponent<Unit>();
            rigidBody = gameObject.GetComponent<Rigidbody>();

            playerSpeed = unit.speed;

            mainCameraTransform = Camera.main.transform;
            lookDir = mainCameraTransform.forward + transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            //destiAngle = curAngle + ...;
            //transform.Look(destiAngle, Vector3.up);
            if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
            {
                SetLookDir(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
            }
            transform.LookAt(lookDir, Vector3.up);
        }

        void FixedUpdate()
        {
            if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
            {
                rigidBody.MovePosition( transform.position +
                                        (mainCameraTransform.forward * Input.GetAxis("Vertical") + mainCameraTransform.right * Input.GetAxis("Horizontal")) * playerSpeed * Time.deltaTime);
            }
        }

        #endregion

        #region Private Methods

        void SetLookDir(float vertical, float horizontal)
        {
            lookDir = (vertical * mainCameraTransform.forward) + (horizontal * mainCameraTransform.right) + transform.position;
            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
        }

        #endregion
    }
}

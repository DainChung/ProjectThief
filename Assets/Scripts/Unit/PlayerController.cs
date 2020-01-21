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

        }

        void FixedUpdate()
        {
            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.2 || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2)
            {
                SetLookDir(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
            }

            //플레이어 캐릭터 회전
            transform.LookAt(lookDir, Vector3.up);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);  //플레이어가 낙하할 때 x축 또는 z축이 회전하는 현상 방지, freezeRotation으로 제어 안 됨

            //플레이어 캐릭터 이동
            //if ((Input.GetButton("Vertical") || Input.GetButton("Horizontal")) && 바닥과 접촉하고 있을 때)로 수정 필요
            if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
            {
                rigidBody.MovePosition( transform.position
                                        + ( mainCameraTransform.forward * Input.GetAxis("Vertical") + mainCameraTransform.right * Input.GetAxis("Horizontal") )
                                        * playerSpeed * Time.deltaTime );
            }
        }

        #endregion

        #region Private Methods

        //카메라가 보고 있는 방향을 전방으로 정함
        void SetLookDir(float vertical, float horizontal)
        {
            lookDir = (vertical * mainCameraTransform.forward) + (horizontal * mainCameraTransform.right) + transform.position;
            lookDir.Set(lookDir.x, transform.position.y, lookDir.z);
        }

        #endregion
    }
}

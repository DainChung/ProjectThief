using UnityEngine;
using System.Collections;

namespace Com.MyCompany.MyGame
{
    public class DemoCameraWork : MonoBehaviour
    {
        #region Private Fields

        [Tooltip("The distance in the local x-z plane to the target")]
        [SerializeField]
        private float distance = 7.0f;


        [Tooltip("The height we want the camera to be above the target")]
        [SerializeField]
        private float height = 3.0f;


        [Tooltip("The Smooth time lag for the height of the camera.")]
        [SerializeField]
        private float heightSmoothLag = 0.3f;

        [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
        [SerializeField]
        private Vector3 centerOffset = Vector3.zero;

        [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
        [SerializeField]
        private bool followOnStart = false;

        Transform cameraTransform;

        bool isFollowing;

        private float heightVelocity;
        private float targetHeight = 100000.0f;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            if (followOnStart)
                OnStartFollowing();
        }

        void LateUpdate()
        {
            if (cameraTransform == null && isFollowing)
                OnStartFollowing();

            if (isFollowing)
                Apply();
        }

        #endregion

        #region Public Methods

        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            isFollowing = true;
            Cut();
        }

        #endregion

        #region Private Methods

        void Apply()
        {
            Vector3 targetCenter = transform.position + centerOffset;

            float originalTargetAngle = transform.eulerAngles.y;
            float currentAngle = cameraTransform.eulerAngles.y;

            float targetAngle = originalTargetAngle;
            currentAngle = targetAngle;
            targetHeight = targetCenter.y + height;

            float currentHeight = cameraTransform.position.y;
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);

            Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);

            cameraTransform.position = targetCenter;
            cameraTransform.position += currentRotation * Vector3.back * distance;

            cameraTransform.position = new Vector3(cameraTransform.position.z, currentHeight, cameraTransform.position.z);
            SetUpRotation(targetCenter);
        }

        void Cut()
        {
            float oldHeightSmooth = heightSmoothLag;
            heightSmoothLag = 0.001f;
            Apply();
            heightSmoothLag = oldHeightSmooth;
        }


        void SetUpRotation(Vector3 centerPos)
        {
            Vector3 cameraPos = cameraTransform.position;
            Vector3 offsetToCenter = centerPos - cameraPos;

            Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));
            Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
            cameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);
        }

        #endregion
    }
}

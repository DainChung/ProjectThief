using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using System.Collections;

using Photon.Pun;

namespace Com.MyCompany.MyGame
{
    public class DemoPlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region IPunObservable Implementation

        //변수 동기화
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {    
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion

        #region Private Fields

        [Tooltip("The Beam GameObject to control")]
        [SerializeField]
        private GameObject beam;

        bool IsFiring;

        #endregion

        #region Public Fields

        [Tooltip("The local player instance.")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (beam == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beam Reference", this);
            }
            else
            {
                beam.SetActive(false);
            }

            if (photonView.IsMine)
            {
                DemoPlayerManager.LocalPlayerInstance = this.gameObject;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            DemoCameraWork _cameraWork = this.gameObject.GetComponent<DemoCameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("DemoCameraWork가 없음");
            }

            #if UNITY_5_4_OR_NEWER

            OnEnable();
            /*
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                this.CalledOnLevelWasLoaded(scene.buildIndex);
            };
            */

            #endif
        }

        void Update()
        {
            if (Health <= 0f)
            {
                DemoGameManager.Instance.LeaveRoom();
            }

            if (photonView.IsMine)
            {
                ProcessInputs();
            }

            if (beam != null && IsFiring != beam.activeInHierarchy)
            {
                beam.SetActive(IsFiring);
            }
        }

        #if UNITY_5_4_OR_NEWER

        /*
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        */

        public override void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        public override void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Level Load");
            Debug.Log(scene.name);
            Debug.Log(mode);

            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position += new Vector3(0f, 5f, 0f);
            }
        }

        #endif

        void CalledOnLevelWasLoaded(int level)
        {
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position += new Vector3(0f, 5f, 0f);
            }
        }

        #endregion

        #region Custom

        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }
            if (Input.GetButtonUp("Fire1"))
            {
                if (IsFiring)
                {
                    IsFiring = false;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Beam"))
            {
                return;
            }

            Health -= 0.1f;
        }

        void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Beam"))
            {
                return;
            }

            Health -= 0.1f * Time.deltaTime;
        }

        #endregion
    }
}

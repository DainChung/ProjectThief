///<summary>
///Script reference: https://doc.photonengine.com/ko-kr/pun/current/demos-and-tutorials/pun-basics-tutorial/
///</summary>

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Com.MyCompany.MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable

        [Tooltip("방에 접속할 수 있는 플레이어의 최대 수")]
        [SerializeField]
        private byte maxPlayerPerRoom = 4;

        #endregion


        #region Private

        bool isConnecting;
        string gameVersion = "1";

        #endregion


        #region MonoBehaviour Callbacks

        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        #endregion


        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

            if (isConnecting)
            {
                //임의의 방에 접속 시도
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);

            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("We load the 'Room1 Demo'");

            PhotonNetwork.LoadLevel("Room1 Demo");
        }

        #endregion


        #region Public Fields

        [Tooltip("The UI Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;

        public void Connect()
        {
            isConnecting = true;

            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            if (PhotonNetwork.IsConnected)
            {
                //임의의 방에 접속, 실패시 방 생성
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                //Photon 서버와 연결
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        void OnApplicationQuit()
        {
            PhotonNetwork.Disconnect();
        }

        public void Quit()
        {        
            Application.Quit();
        }

        #endregion

    }
}


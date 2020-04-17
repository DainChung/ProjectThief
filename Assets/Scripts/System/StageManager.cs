using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;


namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// 출발지점, 탈출지점 관리
    /// 점수 관리
    /// 승패 판정
    /// 일시정지
    /// 메뉴를 통한 종료 등의 관리
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        #region Private Fields

        private GameObject player;
        private GameObject treasure;

        private GameEvent gameEvent;
        private Transform uiRoot;

        #endregion

        #region Public Fields

        public Transform start;
        public Transform[] end;

        [HideInInspector]
        public Dictionary<string, string> buttonToScene = new Dictionary<string, string>();

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            buttonToScene.Add("Button_Home", "Home");
            buttonToScene.Add("Button_Retry", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            buttonToScene.Add("Button_NextLevel", FindNextLevel());
        }

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            uiRoot = GameObject.Find("UI Root").transform.GetChild(0);
            //treasure = GameObject.FindGameObjectWithTag("Treasure");

            gameEvent = new GameEvent(uiRoot);
            //gameEvent.showUI += ShowUIHandler;

            //플레이어 캐릭터를 시작 지점으로 옮김
            player.transform.position = start.position;

            //탈출 지점을 숨김
            foreach (Transform endArea in end)
            {
                endArea.GetComponent<MeshRenderer>().enabled = false;
                endArea.GetComponent<BoxCollider>().enabled = false;
            }
            InitUI();
        }

        void Update()
        {
            if (Input.GetButtonDown("TEST"))
            {
                //gameEvent.OnOffUI(true, "Window_GameResult");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        #endregion

        #region Private Methods

        private string FindNextLevel()
        {
            string curLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            /*
            if(NextLevel == null) this.enable = false;
            else sceneName = FindNextLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            */
            return "Not Yet";
        }

        #endregion

        #region Public Methods
        public void ShowEndArea()
        {
            Random.InitState((int)Time.unscaledTime);
            int index = Random.Range(0, end.Length);
            end[index].GetComponent<BoxCollider>().enabled = true;
            end[index].GetComponent<MeshRenderer>().enabled = true;
        }
        #endregion

        #region UIEvents
        private void InitUI()
        {
            //gameEvent.OnOffUI(false, "Window_Menu");
            gameEvent.OnOffUI(false, "Window_GameResult");
            gameEvent.OnOffUI(false, "Window_Dead");
        }

        public void OnOffButton(bool onoff, string uiName)
        {
            gameEvent.OnOffButton(onoff, uiName);
        }
        public void OnOffButton(bool onoff, string uiName, string buttonName)
        {
            gameEvent.OnOffButton(onoff, uiName, buttonName);
        }

        public void OnOffGameMenu(bool onoff)
        {
            gameEvent.OnOffUI(false, "Window_Menu");
            player.GetComponent<Unit>().lockControl = onoff;
        }
        public void ShowResultWindow(bool isClear)
        {
            gameEvent.ShowResultWindow(isClear);
            player.GetComponent<Unit>().lockControl = true;
            player.GetComponent<CapsuleCollider>().enabled = false;
            player.GetComponent<Rigidbody>().useGravity = false;
            Camera.main.GetComponent<CameraWork>().enabled = false;
        }

        public void LoadScene(string buttonName)
        {
            Debug.Log(buttonName + " : " + buttonToScene[buttonName]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(buttonToScene[buttonName], UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        #endregion
    }

    public class GameEvent
    {
        private List<Transform> ui = new List<Transform>();
        private Dictionary<string, int> uiDic = new Dictionary<string, int>();

        public delegate void ShowUI();
        public event ShowUI showUI;

        public GameEvent(Transform uiRoot)
        {
            for (int i = 0; i < uiRoot.childCount; i++)
            {
                ui.Add(uiRoot.GetChild(i));
                uiDic.Add(uiRoot.GetChild(i).name, i);
            }
        }

        //MonoBehaviour를 상속받은 모든 Class를 비활성화 / 활성화
        private void OnOffUI(bool onoff, Transform uiTR)
        {
            uiTR.GetComponent<MonoBehaviour>().enabled = onoff;
            MonoBehaviour[] list = uiTR.GetComponentsInChildren<MonoBehaviour>();
            for (int i = 0; i < list.Length; i++)
                list[i].enabled = onoff;
        }
        public void OnOffUI(bool onoff, string uiName)
        {
            try
            {
                OnOffUI(onoff, ui[uiDic[uiName]]);
                for (int i = 0; i <= ui[uiDic[uiName]].childCount; i++)
                    OnOffUI(onoff, ui[uiDic[uiName]].GetChild(i));
            }
            catch (System.Exception) { }
        }

        //uiName에 소속된 buttonName 버튼을 비활성화
        public void OnOffButton(bool onoff, string uiName)
        {
            for (int i = 0; i < ui[uiDic[uiName]].childCount; i++)
            {
                try
                {
                    ui[uiDic[uiName]].GetChild(i).GetComponent<UIButton>().isEnabled = onoff;
                }
                catch (System.Exception){ continue; }
            }
        }
        public void OnOffButton(bool onoff, string uiName, string buttonName)
        {
            try
            {
                for (int i = 0; i < ui[uiDic[uiName]].childCount; i++)
                {
                    if (ui[uiDic[uiName]].GetChild(i).name == buttonName)
                    {
                        ui[uiDic[uiName]].GetChild(i).GetComponent<UIButton>().isEnabled = onoff;
                        break;
                    }
                }
            }
            catch (System.Exception) { }
        }

        public void ShowResultWindow(bool isClear)
        {
            if (isClear) OnOffUI(true, "Window_GameResult");
            else         OnOffUI(true, "Window_Dead");
        }
    }

    public static class GameUIIndex
    {
        private static int _sample = 0;

        public static int Sample { get { return _sample; } }
    }
}

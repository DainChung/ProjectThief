using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        #endregion

        void Awake()
        {
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

        public void ShowEndArea()
        {
            Random.InitState((int)Time.unscaledTime);
            int index = Random.Range(0, end.Length);
            end[index].GetComponent<BoxCollider>().enabled = true;
            end[index].GetComponent<MeshRenderer>().enabled = true;
        }

        #region Events
            #region UIEvents

        private void InitUI()
        {
            //gameEvent.OnOffUI(false, "Window_Menu");
            gameEvent.OnOffUI(false, "Window_GameResult");
        }

        public void OnOffMenu(bool onoff)
        {
            gameEvent.OnOffUI(false, "Window_Menu");
        }
        public void ShowResultWindow(bool isClear)
        {
            gameEvent.ShowResultWindow(isClear);
        }
            #endregion
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
            MonoBehaviour[] list = uiTR.GetComponents<MonoBehaviour>();
            for (int i = 0; i < list.Length; i++)
            {
                //Debug.Log(uiTR.name+" : "+i+") "+list[i].GetType().ToString());
                list[i].enabled = onoff;
            }
        }
        public void OnOffUI(bool onoff, string uiName)
        {
            try
            {
                OnOffUI(onoff, ui[uiDic[uiName]]);
                for (int i = 0; i <= ui[uiDic[uiName]].childCount; i++)
                    OnOffUI(onoff, ui[uiDic[uiName]].GetChild(i));
            }
            catch (System.Exception e)
            { }
        }

        public void ShowResultWindow(bool isClear)
        {
            Debug.Log((isClear ? "ShowClearUI" : "ShowDeadUI"));
        }
    }

    public static class GameUIIndex
    {
        private static int _sample = 0;

        public static int Sample { get { return _sample; } }
    }
}

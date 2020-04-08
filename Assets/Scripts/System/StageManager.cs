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
        private Transform canvas;

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
            canvas = GameObject.Find("Canvas").transform;
            //treasure = GameObject.FindGameObjectWithTag("Treasure");

            gameEvent = new GameEvent(canvas);
            //gameEvent.showUI += ShowUIHandler;

            //플레이어 캐릭터를 시작 지점으로 옮김
            player.transform.position = start.position;

            //탈출 지점을 숨김
            foreach (Transform endArea in end)
            {
                endArea.GetComponent<MeshRenderer>().enabled = false;
                endArea.GetComponent<BoxCollider>().enabled = false;
            }
        }

        void Update()
        {
            if (Input.GetButtonDown("TEST"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
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
        public void OnOffMenu(bool onoff)
        {
            gameEvent.ShowMenu(onoff);
        }
        public void ShowResultMenu(bool isClear)
        {
            gameEvent.ShowResultMenu(isClear);
        }
            #endregion
        #endregion
    }

    public class GameEvent
    {
        private List<Transform> ui = new List<Transform>();

        public delegate void ShowUI();
        public event ShowUI showUI;

        public GameEvent(Transform canvas)
        {
            for (int i = 0; i < canvas.childCount; i++)
                ui.Add(canvas.GetChild(i));
        }

        public void ShowMenu(bool onoff)
        {
            Debug.Log("ShowMenu");
        }

        public void ShowResultMenu(bool isClear)
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

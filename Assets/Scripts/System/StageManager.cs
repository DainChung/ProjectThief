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

        private GameObject treasure;
        private float _score = 2.4f;

        #endregion

        #region Public Fields

        public Transform start;
        public Transform[] end;

        public float score { get { return _score; } }
        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            //treasure = GameObject.FindGameObjectWithTag("Treasure");

            //gameEvent.showUI += ShowUIHandler;

            //플레이어 캐릭터를 시작 지점으로 옮김
            GameObject.FindGameObjectWithTag("Player").transform.position = start.position;

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
            {
                //gameEvent.OnOffUI(true, "Window_GameResult");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
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
        public string FindNextLevel()
        {
            string curLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            /*
            if(NextLevel == null) this.enable = false;
            else sceneName = FindNextLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            */
            return "Not Yet";
        }

        public void LoadScene(string buttonName)
        {
            Debug.Log(buttonName + " : " + GetComponent<UIManager>().buttonToScene[buttonName]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(GetComponent<UIManager>().buttonToScene[buttonName], UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        #endregion
    }
}

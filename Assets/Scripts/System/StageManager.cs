using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;


namespace Com.MyCompany.MyGame
{
    #region Sub Class & Struct
    public class GameTime
    {
        private int min;
        private int sec;
        private int milliSec;

        public float time { get { return (float)min + (sec + (float)milliSec * 0.001f) / 60; } }
        public int minutes { get { return min; } }

        public GameTime(System.Diagnostics.Stopwatch timer)
        {
            milliSec = (int)(timer.ElapsedMilliseconds % 1000);
            sec = timer.Elapsed.Seconds;
            min = (int)timer.Elapsed.TotalMinutes;
        }

        public new string ToString()
        {
            return string.Format("{0} : {1}.{2}", min.ToString(), sec.ToString(), milliSec.ToString());
        }
    }

    public struct GameResult
    {
        public float score;
        public GameTime gameTime;
    }
    #endregion

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
        private GameResult _gameResult;

        private System.Diagnostics.Stopwatch gameTimer;

        #endregion

        #region Public Fields

        public Transform start;
        public Transform[] end;

        public GameResult gameResult { get { return _gameResult; } }
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

            _gameResult.score = 0.0f;

            gameTimer = new System.Diagnostics.Stopwatch();
            gameTimer.Start();
        }

        void Update()
        {
            if (Input.GetButtonDown("TEST"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        #endregion

        #region Private Methods
        private void UpdateScore(float amount)
        {
            Debug.Log("Score: " + _gameResult.score + " + " + amount + " = " + (_gameResult.score+amount));
            _gameResult.score += amount;
            if (_gameResult.score > 3) _gameResult.score = 3;
        }
        #endregion

        #region Public Methods
        public void ShowEndArea()
        {
            UpdateScore(Score.GETGOLD);
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

        public void UpdateScore(Score scoreCode)
        {
            switch (scoreCode)
            {
                case Score.ASSASSINATE:
                    UpdateScore(0.5f);
                    break;
                case Score.NORNALKILL:
                    UpdateScore(0.1f);
                    break;
                case Score.GETGOLD:
                    UpdateScore(1);
                    break;
                default:
                    break;
            }
        }

        public void GameClear()
        {
            gameTimer.Stop();
            _gameResult.gameTime = new GameTime(gameTimer);
            UpdateScore(3 - _gameResult.gameTime.time);
            Debug.Log("Min: " + _gameResult.gameTime.time);
            SendMessage("ShowResultWindow", true);
        }
        #endregion
    }
}

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.FileIO;


namespace Com.MyCompany.MyGame.GameSystem
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

        public GameTime(string s)
        {
            float t;
            float.TryParse(s, out t);

            min = (int)t;
            t = (t % 1) * 60;
            sec = (int)t;
            t -= sec;
            milliSec = (int)(t * 1000);
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
        //private List<UnitStat> unitStatDB = new List<UnitStat>();

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
            Time.timeScale = 1;

            try
            {
                //플레이어 캐릭터를 시작 지점으로 옮김
                GameObject.FindGameObjectWithTag("Player").transform.position = start.position;
                //탈출 지점을 숨김
                foreach (Transform endArea in end)
                {
                    endArea.GetComponent<BoxCollider>().enabled = false;
                    endArea.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                }

                _gameResult.score = 0.0f;

                gameTimer = new System.Diagnostics.Stopwatch();
                gameTimer.Start();
            }
            //메인화면
            catch (System.Exception)
            {
                //ReadBestScore
                //Send to Text
            }
        }
        #endregion

        #region Private Methods
        private void UpdateScore(float amount)
        {
            _gameResult.score += amount;
            _gameResult.score = Mathf.Clamp(_gameResult.score, 0, 3);
        }

        private string IndexToSceneName(int i)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            int s = path.LastIndexOf('/');
            string name = path.Substring(s + 1);
            int d = name.LastIndexOf('.');
            return name.Substring(0, d);
        }
        #endregion

        #region Public Methods
        public void ShowEndArea()
        {
            UpdateScore(Score.GETGOLD);
            Random.InitState((int)Time.unscaledTime);
            int index = Random.Range(0, end.Length);
            end[index].GetComponent<BoxCollider>().enabled = true;
            end[index].GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

            GetComponent<UIManager>().SetIndicator("DestiIndicator", end[index]);
        }
        public string FindNextLevel()
        {
            int curLevel = -1;
            string[] s = SceneManager.GetActiveScene().name.Split('e');
            int.TryParse(s[1], out curLevel);

            string nextLevel = string.Format("Stage{0}", (curLevel+1));
            string result = "NULL";

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                if (nextLevel == IndexToSceneName(i))
                    result = nextLevel;

            return result;
        }

        public void LoadScene(string buttonName)
        {
            string sceneName = GetComponent<UIManager>().buttonNameToString[buttonName];
            Debug.Log(buttonName + " : " + sceneName);
            if(sceneName != "NULL") SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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
            string curStage = SceneManager.GetActiveScene().name;
            gameTimer.Stop();
            _gameResult.gameTime = new GameTime(gameTimer);
            UpdateScore(3 - _gameResult.gameTime.time/2);

            bool isBest = false;
            GameResult bestRecord = DataIO.Read("BestRecord.data", curStage);
            if (_gameResult.score >= bestRecord.score || _gameResult.gameTime.time <= bestRecord.gameTime.time)
            {
                DataIO.Write("BestRecord.data", curStage, _gameResult.gameTime.time, _gameResult.score);
                isBest = true;
            }
            GetComponent<UIManager>().ShowResultWindow(true, isBest);
            GetComponent<UIManager>().SetIndicator("DestiIndicator", null);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
        #endregion
    }
}

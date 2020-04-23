using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.UI;

namespace Com.MyCompany.MyGame
{
    public class UIManager : MonoBehaviour
    {
        #region Private Values
        private Transform uiCam;
        private List<UI2DSprite> gameResultStars = new List<UI2DSprite>();

        private GameObject player;
        #endregion

        #region Public Values
        [HideInInspector]   public Dictionary<string, string> buttonToScene = new Dictionary<string, string>();
        #endregion


        #region MonoBehaviour

        void Awake()
        {
            buttonToScene.Add("Button_Home", "Home");
            buttonToScene.Add("Button_Retry", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            buttonToScene.Add("Button_NextLevel", GetComponent<StageManager>().FindNextLevel());
        }

        // Start is called before the first frame update
        void Start()
        {
            uiCam = GameObject.Find("UICamera").transform;

            player = GameObject.Find("Player");
            InitUI();
        }

        // Update is called once per frame
        void Update()
        {
           
        }
        #endregion

        #region Private Methods
        private void InitUI()
        {
            OnOffUI(false, uiCam);

            FillAmountUIName("NearestItemIndicator", 0);
            FillAmountUIName("AssassinateIndicator", 0);
            OnOffUI(true, "Bar_HP");

            for (int i = 0; i < uiCam.Find("Window_GameResult").childCount; i++)
            {
                if (uiCam.Find("Window_GameResult").GetChild(i).name.Contains("Star"))
                    gameResultStars.Add(uiCam.Find("Window_GameResult").GetChild(i).GetComponent<UI2DSprite>());
            }
        }
        private void OnOffUI(bool onoff, Transform uiTR)
        {
            uiTR.GetComponent<UIController>().OnOffAll(onoff);
        }

        private IEnumerator ShowGameResult(GameResult gameResult)
        {
            float score = gameResult.score;

            Debug.Log("Score: " + score + ", Time: " + gameResult.gameTime.ToString());
            SetUILabelText("Window_GameResult", score.ToString(), "Score Value");
            SetUILabelText("Window_GameResult", gameResult.gameTime.ToString(), "Time Value");

            for (int i = 0; i < gameResultStars.Count; i++)
            {
                float maxAmount = Mathf.Clamp01(score);
                while (gameResultStars[i].fillAmount < maxAmount)
                {
                    gameResultStars[i].fillAmount += 0.01f;
                    yield return null;
                }
                score--;
            }

            yield break;
        }
        /// <summary>
        /// 게임 결과창을 활성화
        /// </summary>
        /// <param name="isClear"> true = 클리어, false = Player 사망</param>
        private void _ShowResultWindow(bool isClear)
        {
            SendMessage("StopGameTimer");
            OnOffUI(false, "AssassinateIndicator");
            OnOffUI(false, "NearestItemIndicator");
            if (isClear)
            {
                OnOffUI(true, "Window_GameResult");
                OnOffButtonAll(true, "Window_GameResult");
                StartCoroutine(ShowGameResult(transform.GetComponent<StageManager>().gameResult));
            }
            else
            {
                OnOffUI(true, "Window_Dead");
                OnOffButtonAll(true, "Window_Dead");
            }
        }
        #endregion

        #region Public Methods

        public void OnOffUIWindow(bool enable, string windowName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().OnOffAll(enable);
        }

        public void SetUILabelText(string uiName, string text)
        {
            uiCam.Find(uiName).GetComponent<UIController>().SetText(text);
        }
        public void SetUILabelText(string windowName, string text, string uiName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().SetText(text, uiName);
        }

        public void OnOffGameMenu(bool onoff)
        {
            OnOffUI(false, "Window_Menu");
            player.GetComponent<Unit>().lockControl = onoff;
        }
        public void ShowResultWindow(bool isClear)
        {
            _ShowResultWindow(isClear);

            player.GetComponent<Unit>().lockControl = true;
            player.GetComponent<CapsuleCollider>().enabled = false;
            player.GetComponent<Rigidbody>().useGravity = false;
            Camera.main.GetComponent<CameraWork>().enabled = false;
        }

        /// <summary>
        /// HP바의 길이를 ratio까지 자연스럽게 줄임
        /// </summary>
        /// <param name="ratio">어디까지 줄일지</param>
        /// <param name="index">0: 실제 체력바(밝음), 1: 연출용 체력바(어두움)</param>
        /// <returns></returns>
        public IEnumerator UpdateHPBar(float ratio, int index)
        {
            float r = index + 1;
            float t = 0.0175f;

            while (GetHPBarFillAmount(index) > ratio)
            {
                DecreaseHPBar(r * (1 - Mathf.Cos(t)) / player.GetComponent<Unit>().maxHealth, ratio, index);
                t += 0.01f;
                yield return null;
            }
            if (index == 0)
                StartCoroutine(UpdateHPBar(ratio, 1));
            yield break;
        }

        /// <summary>
        /// uiName과 uiName.Childs에 속한 모든 NGUI 스크립트 비활성화
        /// </summary>
        /// <param name="onoff"> true = 활성화, false = 비활성화</param>
        /// <param name="uiName"> 활성화 / 비활성화 할 UI 이름 </param>
        public void OnOffUI(bool onoff, string uiName)
        {
            try
            {
                OnOffUI(onoff, uiCam.Find(uiName));
            }
            catch (System.Exception) { }
        }

        public void OnOffButton(bool onoff, string uiName)
        {
            uiCam.Find(uiName).GetComponent<UIController>().OnOffUIButton(onoff);
        }

        public void OnOffButton(bool onoff, string windowName, string uiName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().OnOffUIButton(onoff, uiName);
        }

        public void OnOffButtonAll(bool onoff, string windowName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().OnOffUIButtonAll(onoff);
        }
        ///// <summary>
        ///// uiName에 소속된 buttonName 버튼을 활성화 / 비활성화
        ///// </summary>
        ///// <param name="onoff">true = 활성화, false = 비활성화<</param>
        ///// <param name="uiName">활성화 / 비활성화 할 버튼의 parent 이름 </param>
        ///// <param name="buttonName">활성화 / 비활성화 할 버튼 이름 </param>
        //public void OnOffButton(bool onoff, string uiName, string buttonName)
        //{
        //    try
        //    {
        //        for (int i = 0; i < ui[uiDic[uiName]].childCount; i++)
        //        {
        //            if (ui[uiDic[uiName]].GetChild(i).name == buttonName)
        //            {
        //                ui[uiDic[uiName]].GetChild(i).GetComponent<UIButton>().isEnabled = onoff;
        //                break;
        //            }
        //        }
        //    }
        //    catch (System.Exception) { }
        //}

        /// <summary>
        /// HP바를 연속적으로 줄임
        /// </summary>
        /// <param name="amount">얼마나 줄일지</param>
        /// <param name="ratio">어디까지 줄일지</param>
        /// <param name="index">0 = 실제 체력 바, 1 = 나중에 줄어드는 장식용 체력바, ...</param>
        public void DecreaseHPBar(float amount, float ratio, int index)
        {
            string name = ((index == 0) ? "Fill_HP" : "Fill_HP Deco");
            float fill = GetHPBarFillAmount(index);
            fill -= amount;
            if (fill <= ratio) fill = ratio;

            uiCam.Find("Bar_HP").GetComponent<UIController>().SetFillAmount(fill, name);
        }
        public float GetHPBarFillAmount(int index)
        {
            string name = ((index == 0) ? "Fill_HP" : "Fill_HP Deco");
            return uiCam.Find("Bar_HP").GetComponent<UIController>().GetFillAmount(name);
        }

        public void SetFillAmountUIName(string uiName, float setValue)
        {
            uiCam.Find(uiName).GetComponent<UIController>().SetFillAmount(setValue, uiName);
        }
        public void FillAmountUIName(string uiName, float amount)
        {
            uiCam.Find(uiName).GetComponent<UIController>().FillAmount(amount, uiName);
        }
        public bool IsFullUIBasicSprite(string uiName)
        {
            return (uiCam.Find(uiName).GetComponent<UIController>().GetFillAmount() >= 1);
        }

        public void SetFillAmountUIName(string windowName, float setValue, string uiName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().SetFillAmount(setValue, uiName);
        }
        public void FillAmountUIName(string windowName, float amount, string uiName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().FillAmount(amount, uiName);
        }
        public bool IsFullUIBasicSprite(string windowName, string uiName)
        {
            return (uiCam.Find(windowName).GetComponent<UIController>().GetFillAmount(uiName) >= 1);
        }

        public void SetIndicator(string uiName, Transform target)
        {
            OnOffUI((target != null), uiName);

            try
            {
                if (uiCam.Find(uiName).GetComponent<Indicator>().target.GetInstanceID() != target.GetInstanceID())
                    uiCam.Find(uiName).GetComponent<Indicator>().target = target;
                if (target == null)
                {
                    uiCam.Find(uiName).GetComponent<Indicator>().target = null;
                    SetFillAmountUIName(uiName, 0);
                }
            }
            catch(System.Exception){ }
        }

        //uiName이 켜진 상태인지 아닌지 확인
        public bool IsThatUIOn(string windowName, string uiName)
        {
            return uiCam.Find(windowName).GetComponent<UIController>().IsUIOn();
        }
        #endregion
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.UI;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class UIManager : MonoBehaviour
    {
        #region Private Values
        private List<UI2DSprite> gameResultStars = new List<UI2DSprite>();
        private List<UIController> uiWindows = new List<UIController>();
        private Dictionary<string, int> uiWindowsDic = new Dictionary<string, int>();

        private GameObject player;
        #endregion

        #region Public Values
        public Transform uiCam;
        [HideInInspector]   public Dictionary<string, string> buttonNameToString = new Dictionary<string, string>();
        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            buttonNameToString.Add("Button_Home", "MainScene");
            buttonNameToString.Add("Button_Retry", SceneManager.GetActiveScene().name);
            buttonNameToString.Add("Button_NextLevel", GetComponent<StageManager>().FindNextLevel());
            buttonNameToString.Add("Button_Menu", "Window_Menu");
            buttonNameToString.Add("Button_Inventory", "Window_Inventory");
            buttonNameToString.Add("Button_Setting", "Window_Setting");
            buttonNameToString.Add("Button_Stages", "Window_Stage");
            buttonNameToString.Add("Button_Stage1", "Stage1");
            buttonNameToString.Add("Button_Stage2", "Stage2");
        }

        // Start is called before the first frame update
        void Start()
        {
            if (SceneManager.GetActiveScene().name.Contains("Stage"))
                player = GameObject.Find("Player");
            StartCoroutine(InitUI());
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Inventory")) uiCam.Find("Button_Inventory").GetComponent<UIButton>().SendMessage("OnClick");
        }
        #endregion

        #region Private Methods
        IEnumerator InitUI()
        {
            string curScene = SceneManager.GetActiveScene().name;

            for (int i = 0; i < uiCam.childCount; i++)
            {
                uiWindows.Add(uiCam.GetChild(i).GetComponent<UIController>());
                uiWindowsDic.Add(uiWindows[i].name, i);
            }

            yield return null;
            #region 게임 스테이지
            if (curScene.Contains("Stage"))
            {
                OnOffUI(false, uiCam);

                FillAmountUIName("NearestItemIndicator", 0);
                FillAmountUIName("AssassinateIndicator", 0);
                OnOffUI(true, "Bar_HP");
                OnOffUI(true, "Button_Menu");
                OnOffUI(true, "Button_Inventory");
                OnOffUI(true, "Window_EquippedWeapon");
                OnOffUI(true, "Window_MiniMap");
                SetIndicator("DestiIndicator", GameObject.FindGameObjectWithTag("Gold").transform);
                SetIndicator("AssassinateIndicator", null);
                SetIndicator("NearestItemIndicator", null);
                ControlEquippedWeapon(WeaponCode.HAND);

                for (int i = 0; i < uiCam.Find("Window_GameResult").childCount; i++)
                {
                    if (uiCam.Find("Window_GameResult").GetChild(i).name.Contains("Star"))
                        gameResultStars.Add(uiCam.Find("Window_GameResult").GetChild(i).GetComponent<UI2DSprite>());
                }
            }
            #endregion
            #region 메인화면
            else
            {
                OnOffUI(false, uiCam);
                OnOffUIWindow(true, "Window_Menu");
                OnOffUIWindow(true, "BackGround");
            }
            #endregion
        }
        private void OnOffUI(bool onoff, Transform uiTR)
        {
            if (uiTR == uiCam)
            {
                for (int i = 0; i < uiWindows.Count; i++)
                    uiWindows[i].OnOffAll(onoff);
            }
            else
            {
                try {  uiWindows[uiWindowsDic[uiTR.name]].OnOffAll(onoff);  }
                catch (System.Exception) { }
            }
        }

        private IEnumerator ShowGameResult(GameResult gameResult, bool isBestRecord)
        {
            string window = "Window_GameResult";
            OnOffUI(false, window, "BestText");
            OnOffButtonAll(false, window);

            float addAmount = 0.008f;
            float score = gameResult.score;

            SetUILabelText(window, ((int)(gameResult.score * 1000)).ToString(), "Score Value");
            SetUILabelText(window, gameResult.gameTime.ToString(), "Time Value");

            for (int i = 0; i < gameResultStars.Count; i++)
            {
                float maxAmount = Mathf.Clamp01(score);
                while (gameResultStars[i].fillAmount < maxAmount)
                {
                    gameResultStars[i].fillAmount += addAmount;
                    if (gameResultStars[i].fillAmount > maxAmount) gameResultStars[i].fillAmount = maxAmount;
                    //마우스 좌클릭하면 빠른 결과
                    if (Input.GetMouseButtonDown(0)) addAmount = 1;
                    yield return null;
                }
                score--;
            }

            if (isBestRecord) OnOffUI(true, window, "BestText");

            OnOffButtonAll(true, window);

            yield break;
        }
        /// <summary>
        /// 게임 결과창을 활성화
        /// </summary>
        /// <param name="isClear"> true = 클리어, false = Player 사망</param>
        private void _ShowResultWindow(bool isClear, bool isBestRecord)
        {
            OnOffButton(false, "Button_Inventory");
            OnOffButton(false, "Button_Menu");
            OnOffUI(false, "AssassinateIndicator");
            OnOffUI(false, "NearestItemIndicator");
            if (isClear)
            {
                OnOffUI(true, "Window_GameResult");
                OnOffButtonAll(true, "Window_GameResult");
                StartCoroutine(ShowGameResult(transform.GetComponent<StageManager>().gameResult, isBestRecord));
            }
            else
            {
                OnOffUI(true, "Window_Dead");
                OnOffButtonAll(true, "Window_Dead");
            }
        }

        private void SetMainStagesWindow()
        {
            for (int i = 1; i < 3; i++)
            {
                string stageName = string.Format("Stage{0}", i);
                string nextStageName = string.Format("Stage{0}", i+1);
                GameResult fileData = FileIO.DataIO.Read("BestRecord.data", stageName);
                int score = (int)(fileData.score * 1000);

                UIController ui = uiCam.Find("Window_Stage").Find(stageName).Find("Record").GetComponent<UIController>();

                //클리어 한 적 없음
                if (score == 0 && fileData.gameTime.minutes > 9000)
                {
                    ui.SetText("NULL", "BestTime");
                    ui.SetText("NULL", "BestScore");
                    //첫 스테이지가 아니면 잠금
                    try { uiCam.Find("Window_Stage").Find(string.Format("Button_{0}", nextStageName)).GetComponent<UIController>().OnOffUIButton(false); }
                    catch (System.Exception) { }
                }
                //클리어한 스테이지
                else
                {
                    ui.SetText(fileData.gameTime.ToString(), "BestTime");
                    ui.SetText(score.ToString(), "BestScore");
                }
            }
        }
        #endregion

        #region Public Methods

        public void SetUISliderValue(string windowName, string uiName, float v)
        {
            uiCam.Find(windowName).Find(uiName).GetComponent<UISlider>().value = Mathf.Clamp01(v);
        }

        /// <summary>
        /// buttonName 버튼으로 특정 Window를 열 때 사용
        /// </summary>
        /// <param name="enable">true = Window 활성화, false = Window 비활성화</param>
        /// <param name="buttonName">버튼 이름</param>
        public void OnOffUIWindowByButton(bool enable, string buttonName)
        {
            string windowName = buttonNameToString[buttonName];

            OnOffUIWindow(enable, windowName);
            try {  OnOffButton(!enable, buttonName);  }
            catch (System.Exception) { }
            if (windowName == "Window_Menu" || windowName == "Window_Inventory")
            {
                Time.timeScale = (enable ? 0 : 1);
                try { OnOffButton(!enable, "Button_Menu"); }
                catch { OnOffButton(!enable, "Window_Menu", buttonName); }
                try { OnOffButton(!enable, "Button_Inventory"); }
                catch (System.Exception) {}
            }
            else if (windowName == "Window_Setting") OnOffButton(!enable, "Window_Menu", "Button_Setting");
            else if (windowName == "Window_Stage") SetMainStagesWindow();
        }

        public void OnOffUIWindow(bool enable, string windowName)
        {
            try {  uiWindows[uiWindowsDic[windowName]].OnOffAll(enable); }
            catch (System.Exception) { }
        }

        public void SetUILabelText(string uiName, string text)
        {
            uiWindows[uiWindowsDic[uiName]].SetText(text);
        }
        public void SetUILabelText(string windowName, string text, string uiName)
        {
            try
            {
                uiWindows[uiWindowsDic[windowName]].SetText(text, uiName);
            }
            catch (System.NullReferenceException)
            {
                //uiCam이 정의되지 않았을 때
                GameObject.Find("UICamera").transform.Find(windowName).GetComponent<UIController>().SetText(text, uiName);
            }
        }

        public void OnOffGameMenu(bool onoff)
        {
            OnOffUI(false, "Window_Menu");
            player.GetComponent<Unit>().lockControl = onoff;
        }
        public void ShowResultWindow(bool isClear, bool isBestRecord)
        {
            _ShowResultWindow(isClear, isBestRecord);
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

            float playerMaxHP = player.GetComponent<Unit>().maxHealth;
            while (GetHPBarFillAmount(index) > ratio)
            {
                DecreaseHPBar(r * (1 - Mathf.Cos(t)) / playerMaxHP, ratio, index);
                t += 0.01f;
                yield return null;
            }
            if (index == 0)
                StartCoroutine(UpdateHPBar(ratio, 1));
            yield break;
        }

        public void ControlEquippedWeapon(WeaponCode curWeapon)
        {
            Transform windowInventory = null;
            try
            {
                windowInventory = uiCam.Find("Window_EquippedWeapon");
            }
            catch (System.NullReferenceException)
            {
                windowInventory = GameObject.Find("UICamera").transform.Find("Window_EquippedWeapon");
            }
            finally
            {
                windowInventory.GetComponent<UIController>().OnOffChildren(false);
                UIController curWeaponIcon = windowInventory.Find(curWeapon.ToString()).GetComponent<UIController>();
                curWeaponIcon.OnOffUI(true);
                curWeaponIcon.OnOffChildren(curWeaponIcon.GetFillAmount("CoolTime") != 0);
            }
        }
        public void ControlEquippedWeaponCoolTime(WeaponCode curWeapon)
        {
            UIController curWeaponIcon = null;
            try
            {
                curWeaponIcon = uiCam.Find("Window_EquippedWeapon").Find(curWeapon.ToString()).GetComponent<UIController>();
            }
            catch (System.NullReferenceException)
            {
                curWeaponIcon = GameObject.Find("UICamera").transform.Find("Window_EquippedWeapon").Find(curWeapon.ToString()).GetComponent<UIController>();
            }
            finally
            {
                curWeaponIcon.OnOffChildren(true);
            }
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
        public void OnOffUI(bool onoff, string windowName, string uiName)
        {
            try
            {
                uiCam.Find(windowName).Find(uiName).GetComponent<UIController>().OnOffAll(onoff);
            }
            catch (System.Exception) { }
        }

        public void OnOffButton(bool onoff, string uiName)
        {
            uiWindows[uiWindowsDic[uiName]].OnOffUIButton(onoff);
        }

        public void OnOffButton(bool onoff, string windowName, string uiName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().OnOffUIButton(onoff, uiName);
        }

        public void OnOffButtonAll(bool onoff, string windowName)
        {
            uiCam.Find(windowName).GetComponent<UIController>().OnOffUIButtonAll(onoff);
        }

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

            try
            {
                uiWindows[uiWindowsDic["Bar_HP"]].SetFillAmount(fill, name);
            }
            catch (KeyNotFoundException)
            {
                uiCam.Find("Bar_HP").GetComponent<UIController>().SetFillAmount(fill, name);
            }
        }
        public float GetHPBarFillAmount(int index)
        {
            string name = ((index == 0) ? "Fill_HP" : "Fill_HP Deco");
            try
            {
                return uiWindows[uiWindowsDic["Bar_HP"]].GetFillAmount(name);
            }
            catch (KeyNotFoundException)
            {
                return uiCam.Find("Bar_HP").GetComponent<UIController>().GetFillAmount(name);
            }
        }

        public void SetFillAmountUIName(string uiName, float setValue)
        {
            try
            {
                uiWindows[uiWindowsDic[uiName]].SetFillAmount(setValue, uiName);
            }
            catch (KeyNotFoundException)
            {
                uiCam.Find(uiName).GetComponent<UIController>().SetFillAmount(setValue, uiName);
            }
        }
        public void FillAmountUIName(string uiName, float amount)
        {
            try
            {
                uiWindows[uiWindowsDic[uiName]].FillAmount(amount, uiName);
            }
            catch (KeyNotFoundException)
            {
                uiCam.Find(uiName).GetComponent<UIController>().FillAmount(amount, uiName);
            }
        }
        public bool IsFullUIBasicSprite(string uiName)
        {
            try
            {
                return (uiWindows[uiWindowsDic[uiName]].GetFillAmount() >= 1);
            }
            catch (KeyNotFoundException)
            {
                return (uiCam.Find(uiName).GetComponent<UIController>().GetFillAmount() >= 1);
            }
        }

        public void SetFillAmountUIName(string windowName, float setValue, WeaponCode weaponCode)
        {
            try
            {
                uiWindows[uiWindowsDic[windowName]].SetFillAmount(setValue, weaponCode.ToString());
            }
            catch (KeyNotFoundException)
            {
                uiCam.Find(windowName).Find(weaponCode.ToString()).GetComponent<UIController>().SetFillAmount(setValue);
            }
        }
        public bool IsFullUIBasicSprite(string windowName, string uiName)
        {
            return (uiCam.Find(windowName).GetComponent<UIController>().GetFillAmount(uiName) >= 1);
        }

        public void SetColorUIName(string windowName, Color color, WeaponCode weaponCode)
        {
            uiCam.Find(windowName).Find(weaponCode.ToString()).GetComponent<UIController>().SetColor(color);
        }

        public void SetIndicator(string uiName, Transform target)
        {
            Indicator indicator = uiWindows[uiWindowsDic[uiName]].GetComponent<Indicator>();
            OnOffUI((target != null && target.gameObject.activeInHierarchy), indicator.transform);

            try
            {
                indicator.target = target;
                if (target == null || !target.gameObject.activeInHierarchy)
                {
                    indicator.target = null;
                    SetFillAmountUIName(uiName, 0);
                }
            }
            catch(System.Exception){ }
        }

        public UIController GetUIController(string windowName)
        {
            try
            {
                return uiWindows[uiWindowsDic[windowName]];
            }
            catch (KeyNotFoundException)
            {
                return uiCam.Find(windowName).GetComponent<UIController>();
            }
        }
        public UIController GetUIController(string windowName, string uiName)
        {
            return uiCam.Find(windowName).Find(uiName).GetComponent<UIController>();
        }

        //uiName이 켜진 상태인지 아닌지 확인
        public bool IsThatUIOn(string windowName, string uiName)
        {
            return uiCam.Find(windowName).GetComponent<UIController>().IsUIOn();
        }
        #endregion
    }
}

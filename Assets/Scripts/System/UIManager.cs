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
        private List<Transform> ui = new List<Transform>();
        private Dictionary<string, int> uiDic = new Dictionary<string, int>();

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
            SetUIDic();

            player = GameObject.Find("Player");
            InitUI();
        }

        // Update is called once per frame
        void Update()
        { 
        }
        #endregion

        #region Private Methods
        private void SetUIDic()
        {
            if (ui.Count != 0) ui.Clear();
            if (uiDic.Count != 0) uiDic.Clear();

            for (int i = 0; i < uiCam.childCount; i++)
            {
                ui.Add(uiCam.GetChild(i));
                uiDic.Add(uiCam.GetChild(i).name, i);
            }
        }
        private void InitUI()
        {
            for (int i = 0; i < ui.Count; i++)
                OnOffUI(false, ui[i]);
            FillTextureUIName("NearestItemIndicator", 0);
            FillTextureUIName("AssassinateIndicator", 0);
            OnOffUI(true, "Bar_HP");
        }
        private void OnOffUI(bool onoff, Transform uiTR)
        {
            uiTR.GetComponent<MonoBehaviour>().enabled = onoff;
            MonoBehaviour[] list = uiTR.GetComponentsInChildren<MonoBehaviour>();
            for (int i = 0; i < list.Length; i++)
                list[i].enabled = onoff;
        }
        #endregion

        #region Public Methods

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
        /// MonoBehaviour를 상속받은 모든 Class를 활성화 / 비활성화
        /// </summary>
        /// <param name="onoff"> true = 활성화, false = 비활성화</param>
        /// <param name="uiName"> 활성화 / 비활성화 할 UI 이름 </param>
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

        public void OnOffButton(bool onoff, string uiName)
        {
            for (int i = 0; i < ui[uiDic[uiName]].childCount; i++)
            {
                try
                {
                    ui[uiDic[uiName]].GetChild(i).GetComponent<UIButton>().isEnabled = onoff;
                }
                catch (System.Exception) { continue; }
            }
        }
        /// <summary>
        /// uiName에 소속된 buttonName 버튼을 활성화 / 비활성화
        /// </summary>
        /// <param name="onoff">true = 활성화, false = 비활성화<</param>
        /// <param name="uiName">활성화 / 비활성화 할 버튼의 parent 이름 </param>
        /// <param name="buttonName">활성화 / 비활성화 할 버튼 이름 </param>
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

        /// <summary>
        /// 게임 결과창을 활성화
        /// </summary>
        /// <param name="isClear"> true = 클리어, false = Player 사망</param>
        public void _ShowResultWindow(bool isClear)
        {
            if (isClear) OnOffUI(true, "Window_GameResult");
            else OnOffUI(true, "Window_Dead");
        }

        /// <summary>
        /// HP바를 연속적으로 줄임
        /// </summary>
        /// <param name="amount">얼마나 줄일지</param>
        /// <param name="ratio">어디까지 줄일지</param>
        /// <param name="index">0 = 실제 체력 바, 1 = 나중에 줄어드는 장식용 체력바, ...</param>
        public void DecreaseHPBar(float amount, float ratio, int index)
        {
            float fill = ui[uiDic["Bar_HP"]].GetChild(index).GetComponent<UI2DSprite>().fillAmount;

            fill -= amount;
            if (fill <= ratio) fill = ratio;

            ui[uiDic["Bar_HP"]].GetChild(index).GetComponent<UI2DSprite>().fillAmount = fill;
        }
        public float GetHPBarFillAmount(int index)
        {
            return ui[uiDic["Bar_HP"]].GetChild(index).GetComponent<UI2DSprite>().fillAmount;
        }

        public void SetFillTextureUIName(string uiName, float fillAmount)
        {
            ui[uiDic[uiName]].GetComponent<UITexture>().fillAmount = fillAmount;
        }
        public void FillTextureUIName(string uiName, float amount)
        {
            //Debug.Log(uiName + " : " + ui[uiDic[uiName]].GetComponent<UITexture>().fillAmount + " + " + amount);
            ui[uiDic[uiName]].GetComponent<UITexture>().fillAmount += amount;
        }
        public bool IsFullTexture(string uiName)
        {
            return (ui[uiDic[uiName]].GetComponent<UITexture>().fillAmount >= 1);
        }

        public void SetIndicator(string uiName, Transform target)
        {
            OnOffUI((target != null), uiName);

            try
            {
                if (ui[uiDic[uiName]].GetComponent<Indicator>().target.GetInstanceID() != target.GetInstanceID())
                    ui[uiDic[uiName]].GetComponent<Indicator>().target = target;
                if (target == null)
                {
                    ui[uiDic[uiName]].GetComponent<Indicator>().target = null;
                    SetFillTextureUIName(uiName, 0);
                }
            }
            catch(System.Exception){ }
        }

        //uiName이 켜진 상태인지 아닌지 확인
        public bool IsThatUIOn(string uiName)
        {
            return ui[uiDic[uiName]].GetComponent<MonoBehaviour>().enabled;
        }
        #endregion
    }
}

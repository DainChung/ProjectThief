using System.Collections;
using UnityEngine;

using Com.MyCompany.MyGame.GameSystem;

namespace Com.MyCompany.MyGame.UI
{
    public class UIController : UI
    {
        private AudioManager audioManager;

        void Start()
        {
            if (transform.name.Contains("Button"))
            {
                audioManager = GameObject.FindWithTag("Manager").GetComponent<AudioManager>();
                StartCoroutine(GetButtonState());
            }
        }

        public IEnumerator GetButtonState()
        {
            while (GetComponent<UIButton>().state != UIButtonColor.State.Hover) yield return null;
            audioManager.PlayAudio("Hover");
            while (GetComponent<UIButton>().state == UIButtonColor.State.Hover) yield return null;
            StartCoroutine(GetButtonState());
            yield break;
        }

        #region Public Methods
        public void OffParent()
        {
            transform.parent.GetComponent<UIController>().OnOffAll(false);
        }
        public void OnParent()
        {
            transform.parent.GetComponent<UIController>().OnOffAll(true);
        }

        public void OnOffAll(bool enable)
        {
            try
            {
                base.OnOffUI(enable);
                OnOffChildren(enable);
                if (enable) OnOffIcon();
                try { transform.GetComponent<Indicator>().enabled = enable; }
                catch (System.NullReferenceException) { }
            }
            catch (System.Exception) { }
        }

        public void OnOffChildren(bool enable)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                try
                {
                    transform.GetChild(i).GetComponent<UIController>().OnOffAll(enable);
                    transform.GetChild(i).GetComponent<Indicator>().enabled = enable;
                }
                catch (System.Exception) { }
            }
        }

        public void OnOffUIButtonAll(bool enable)
        {
            try
            {
                if (audioManager.GetComponent<UIManager>().buttonNameToString[transform.name] == "NULL") base.OnOffUIButton(false);
                else base.OnOffUIButton(enable);
            }
            catch (System.Exception){}
            finally
            {
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<UIController>().OnOffUIButtonAll(enable);
            }
        }
        public void OnOffUIButton(bool enable, string buttonName)
        {
            if (transform.name != buttonName) transform.Find(buttonName).GetComponent<UIController>().OnOffUIButton(enable, buttonName);
            else base.OnOffUIButton(enable);
        }

        /// <summary>
        /// transform.name == uiName이면 amount만큼 UITexture를 채워줌, 
        /// transform.name != uiName이면 transform의 Child 중 uiName과 일치하는 UITexture를 채워줌
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="uiName"></param>
        public void FillAmount(float amount, string uiName)
        {
            try
            {
                if (transform.name != uiName) transform.Find(uiName).GetComponent<UIController>().FillAmount(amount, uiName);
                else base.FillAmount(amount);
            }
            catch (System.NullReferenceException)
            {
                for(int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<UIController>().FillAmount(amount, uiName);
            }
        }
        public void SetFillAmount(float setValue, string uiName)
        {
            try
            {
                if (transform.name != uiName) transform.Find(uiName).GetComponent<UIController>().SetFillAmount(setValue, uiName);
                else base.SetFillAmount(setValue);
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<UIController>().SetFillAmount(setValue, uiName);
            }
        }
        public float GetFillAmount(string uiName)
        {
            float result = -1;

            try
            {
                if (transform.name != uiName) result = transform.Find(uiName).GetComponent<UIController>().GetFillAmount(uiName);
                else result = base.GetFillAmount();
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    result = transform.GetChild(i).GetComponent<UIController>().GetFillAmount(uiName);
                    if (result != -1) break;
                }
            }

            return result;
        }

        public void SetText(string text, string uiName)
        {
            try
            {
                if (transform.name != uiName) transform.Find(uiName).GetComponent<UIController>().SetText(text, uiName);
                else
                {
                    if (text == "NULL") base.SetText(text, 45);
                    else base.SetText(text);
                }
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    try{ transform.GetChild(i).GetComponent<UIController>().SetText(text, uiName);}
                    catch (System.NullReferenceException) { }
                }
            }
        }
        public string GetText(string uiName)
        {
            string result = "NULL";

            try
            {
                if (transform.name != uiName) result = transform.Find(uiName).GetComponent<UIController>().GetText(uiName);
                else result = base.GetText();
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    result = transform.GetChild(i).GetComponent<UIController>().GetText(uiName);
                    if (result.CompareTo("NULL") != 0) break;
                }
            }

            return result;
        }

        /// <summary>
        /// Child이름 중 Icon_On, Icon_Off인 것만 제어
        /// </summary>
        /// <param name="value"> 0 == Icon_Off만 활성화, 1 == Icon_On만 활성화</param>
        public void ControlIconByButton(float value)
        {
            value = (value > 0) ? 0.0f : 1.0f;
            bool _enable = (value > 0) ? true : false;

            try
            {
                transform.Find("Icon_On").GetComponent<UIController>().OnOffUI(_enable);
                transform.Find("Icon_Off").GetComponent<UIController>().OnOffUI(!_enable);
            }
            catch (System.Exception) { Debug.Log(transform.name+ "의 Child에 이름이 Icon_On, Icon_Off인 물체가 필요합니다. 두 물체에 UIController가 필요합니다."); }
        }
        public void ControlSliderByButton(bool enable)
        {
            try { GetComponent<UISlider>().value = enable ? 1 : 0; }
            catch (System.Exception) { Debug.Log(transform.name+"에 UISlider가 없습니다."); }
        }
        #endregion

        #region Private Methods
        private void OnOffIcon()
        {
            Transform iconOn = null;
            Transform iconOff = null;

            try
            {
                iconOn = transform.Find("Icon_On");
                iconOff = transform.Find("Icon_Off");

                float value = GameObject.FindGameObjectWithTag("Manager").GetComponent<Com.MyCompany.MyGame.GameSystem.FileManager>().audioVolume;
                bool _enable = (value > 0) ? true : false;

                iconOn.GetComponent<UIController>().OnOffUI(_enable);
                iconOff.GetComponent<UIController>().OnOffUI(!_enable);
            }
            catch (System.Exception){}
        }
        #endregion
    }
}

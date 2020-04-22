﻿using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class UIController : UI
    {

        public void OnOffAll(bool enable)
        {
            base.OnOffUI(enable);
            try
            {
                transform.GetComponent<Indicator>().enabled = enable;
            }
            catch(System.NullReferenceException) { }
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
                base.OnOffUIButton(enable);
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
            try
            {
                if (transform.name != buttonName) throw new System.NullReferenceException();
                else base.OnOffUIButton(enable);
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<UIController>().OnOffUIButton(enable, buttonName);
            }
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
                if (transform.name != uiName) throw new System.NullReferenceException();
                else base.FillAmount(amount);
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<UIController>().FillAmount(amount, uiName);
            }
        }
        public void SetFillAmount(float setValue, string uiName)
        {
            try
            {
                if (transform.name != uiName) throw new System.NullReferenceException();
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
                if (transform.name != uiName) throw new System.NullReferenceException();
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
                if (transform.name != uiName) throw new System.NullReferenceException();
                else base.SetText(text);
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<UIController>().SetText(text, uiName);
            }
        }
    }
}

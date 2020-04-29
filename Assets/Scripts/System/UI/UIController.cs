using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public class UIController : UI
    {
        public void OnOffAll(bool enable)
        {
            base.OnOffUI(enable);
            OnOffChildren(enable);
            try
            {
                transform.GetComponent<Indicator>().enabled = enable;
            }
            catch(System.NullReferenceException) { }
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
                else base.SetText(text);
            }
            catch (System.NullReferenceException)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    try
                    {
                        transform.GetChild(i).GetComponent<UIController>().SetText(text, uiName);
                    }
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
    }
}

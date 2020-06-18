using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MyCompany.MyGame.UI
{
    public abstract class UI : MonoBehaviour
    {
        float maxW = 38424;
        float maxH = 18490;

        UIRect[] uiRects;
        UIWidgetContainer[] uiWidgetConts;
        RawImage rawImage;
        BoxCollider boxCollider;
        UIButton uiButton;
        UIBasicSprite uiBasicSprite;
        UILabel uiLabel;
        UI2DSprite ui2DSprite;

        protected void InitMaxWMaxH(Vector3 pos)
        {
            maxW = pos.x / 4;
            maxH = pos.y / 4;
        }

        protected void InitComponents()
        {
            uiRects = GetComponents<UIRect>();
            uiWidgetConts = GetComponents<UIWidgetContainer>();
            TryGetComponent<RawImage>(out rawImage);
            TryGetComponent<BoxCollider>(out boxCollider);
            TryGetComponent<UIButton>(out uiButton);
            TryGetComponent<UIBasicSprite>(out uiBasicSprite);
            TryGetComponent<UILabel>(out uiLabel);
            TryGetComponent<UI2DSprite>(out ui2DSprite);
        }

        /// <summary>
        /// destiPos 위치로 UI를 이동
        /// </summary>
        /// <param name="destiPos"> WorldPos </param>
        protected virtual void Move(Vector3 destiPos)
        {
            destiPos = Camera.main.WorldToViewportPoint(destiPos);

            //화면을 넘어가지 않도록 함
            float x = 2 * (Mathf.Clamp(destiPos.x, 0.05f, 0.95f) - 0.5f);
            float y = 2 * (Mathf.Clamp(destiPos.y, 0.05f, 0.95f) - 0.5f);
            if (destiPos.z < 0) y = -0.95f;
            destiPos.Set(x * maxW, y * maxH, 0);
            transform.localPosition = destiPos;
        }
        protected virtual void RotateY(float destiRotEulerY)
        {
            transform.rotation = Quaternion.Euler(0, destiRotEulerY, 0);
        }

        /// <summary>
        /// NGUI Unity2DSprite, NGUI Texture, UI Button을 모두 활성화 / 비활성화 함
        /// </summary>
        /// <param name="enable">true = 활성화, false = 비활성화</param>
        public virtual void OnOffUI(bool enable)
        {
            try { boxCollider.enabled = enable; }
            catch (System.Exception) { }
            try { rawImage.enabled = enable; }
            catch (System.Exception) { }

            try
            {
                for (int i = 0; i < uiRects.Length; i++)
                    uiRects[i].enabled = enable;
            }
            catch (System.NullReferenceException)
            {
                UIRect[] uiR = GetComponents<UIRect>();

                for (int i = 0; i < uiR.Length; i++)
                    uiR[i].enabled = enable;
            }

            try
            {
                for (int j = 0; j < uiWidgetConts.Length; j++)
                {
                    uiWidgetConts[j].enabled = enable;
                    try { OnOffUIButton(enable); }
                    catch (System.Exception) { }
                }
            }
            catch (System.NullReferenceException)
            {
                UIWidgetContainer[] uiW = GetComponents<UIWidgetContainer>();

                for (int i = 0; i < uiW.Length; i++)
                {
                    uiW[i].enabled = enable;
                    try { OnOffUIButton(enable); }
                    catch (System.Exception) { }
                }
            }
        }
        public virtual void OnOffUIButton(bool enable)
        {
            try
            {
                uiButton.isEnabled = enable;
                boxCollider.enabled = enable;
            }
            catch (System.NullReferenceException)
            {
                GetComponent<UIButton>().isEnabled = enable;
                GetComponent<BoxCollider>().enabled = enable;
            }
            catch (System.Exception e)
            {
                Debug.Log(transform.name + "." + this.name + ".OnOffUIButton() Exception : " + e);
            }
        }
        public virtual void OnOffUIButtonAuto()
        {
            try
            {
                if (uiButton.isEnabled)
                {
                    uiButton.isEnabled = false;
                    boxCollider.enabled = false;
                }
                else
                {
                    uiButton.isEnabled = true;
                    boxCollider.enabled = true;
                }
            }
            catch (System.NullReferenceException)
            {
                UIButton uiB = GetComponent<UIButton>();
                BoxCollider boxC = GetComponent<BoxCollider>();

                if (uiB.isEnabled)
                {
                    uiB.isEnabled = false;
                    boxC.enabled = false;
                }
                else
                {
                    uiB.isEnabled = true;
                    boxC.enabled = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(transform.name + "." + this.name + ".OnOffUIButtonAuto() Exception : " + e);
            }
        }
        /// <summary>
        /// transform이 NGUI Texture 또는 NGUI Unity2DSprite를 갖고 있어야 작동,
        /// fillAmount += amount
        /// </summary>
        /// <param name="amount">fillAmount에 더할 값</param>
        public virtual void FillAmount(float amount)
        {
            try { uiBasicSprite.fillAmount += amount; }
            catch (System.NullReferenceException)
            {
                GetComponent<UIBasicSprite>().fillAmount += amount;
            }
        }
        /// <summary>
        /// transform이 UITexture 또는 UI2DSprite를 갖고 있어야 작동,
        /// filAmount = setValue
        /// </summary>
        /// <param name="setValue">fillAmount값 설정</param>
        public virtual void SetFillAmount(float setValue)
        {
            try { uiBasicSprite.fillAmount = setValue; }
            catch (System.NullReferenceException)
            {
                GetComponent<UIBasicSprite>().fillAmount = setValue;
            }
        }
        /// <summary>
        /// transform이 UITexture 또는 UI2DSprite를 갖고 있어야 작동,
        /// fillAmount값을 반환
        /// </summary>
        /// <returns></returns>
        public virtual float GetFillAmount()
        {
            try { return uiBasicSprite.fillAmount; }
            catch (System.NullReferenceException)
            {
                return GetComponent<UIBasicSprite>().fillAmount;
            }
        }

        public virtual void SetText(string text)
        {
            try { uiLabel.text = text; }
            catch (System.NullReferenceException)
            {
                GetComponent<UILabel>().text = text;
            }
        }
        public virtual void SetText(string text, int size)
        {
            try
            {
                uiLabel.fontSize = size;
                uiLabel.text = text;
            }
            catch (System.NullReferenceException)
            {
                UILabel uiL = GetComponent<UILabel>();
                uiL.fontSize = size;
                uiL.text = text;
            }
        }
        public virtual string GetText()
        {
            try { return uiLabel.text; }
            catch (System.NullReferenceException)
            {
                return GetComponent<UILabel>().text;
            }
        }

        public virtual void SetColor(Color color)
        {
            try   { ui2DSprite.color = color; }
            catch (System.NullReferenceException)
            {
                GetComponent<UI2DSprite>().color = color;
            }
        }

        public bool IsUIOn()
        {
            int result = -1;

            try
            {
                result = (uiRects[0].enabled) ? 1 : 0;
            }
            catch (System.Exception)
            {
                result = (uiWidgetConts[0].enabled) ? 1 : 0;
            }

            if (result == -1) Debug.Log("There is no UIRect OR UIWidgetContainer");

            return (result == 1) ? true : false;
        }
    }
}
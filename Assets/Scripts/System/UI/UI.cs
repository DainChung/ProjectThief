using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame.UI
{
    public abstract class UI : MonoBehaviour
    {
        float maxW = 38424;
        float maxH = 18490;

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
            UIRect[] uiRects = GetComponents<UIRect>();
            UIWidgetContainer[] uiWidgetConts = GetComponents<UIWidgetContainer>();
            UnityEngine.UI.RawImage rawImage;
            TryGetComponent<UnityEngine.UI.RawImage>(out rawImage);

            try { GetComponent<BoxCollider>().enabled = enable; }
            catch (System.Exception) { }

            for (int i = 0; i < uiRects.Length; i++) uiRects[i].enabled = enable;

            for (int j = 0; j < uiWidgetConts.Length; j++)
            {
                uiWidgetConts[j].enabled = enable;
                try { OnOffUIButton(enable);}
                catch (System.Exception) { }
            }

            try { rawImage.enabled = enable; }
            catch (System.Exception) { }
        }
        public virtual void OnOffUIButton(bool enable)
        {
            try
            {
                GetComponent<UIButton>().isEnabled = enable;
                GetComponent<BoxCollider>().enabled = enable;
            }
            catch (System.Exception) { }
        }
        public virtual void OnOffUIButtonAuto()
        {
            try
            {
                if (GetComponent<UIButton>().isEnabled)
                {
                    GetComponent<UIButton>().isEnabled = false;
                    GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    GetComponent<UIButton>().isEnabled = true;
                    GetComponent<BoxCollider>().enabled = true;
                }
            }
            catch (System.Exception) { }
        }
        /// <summary>
        /// transform이 NGUI Texture 또는 NGUI Unity2DSprite를 갖고 있어야 작동,
        /// fillAmount += amount
        /// </summary>
        /// <param name="amount">fillAmount에 더할 값</param>
        public virtual void FillAmount(float amount)
        {
            transform.GetComponent<UIBasicSprite>().fillAmount += amount;
        }
        /// <summary>
        /// transform이 UITexture 또는 UI2DSprite를 갖고 있어야 작동,
        /// filAmount = setValue
        /// </summary>
        /// <param name="setValue">fillAmount값 설정</param>
        public virtual void SetFillAmount(float setValue)
        {
            transform.GetComponent<UIBasicSprite>().fillAmount = setValue;
        }
        /// <summary>
        /// transform이 UITexture 또는 UI2DSprite를 갖고 있어야 작동,
        /// fillAmount값을 반환
        /// </summary>
        /// <returns></returns>
        public virtual float GetFillAmount()
        {
            return transform.GetComponent<UIBasicSprite>().fillAmount;
        }

        public virtual void SetText(string text)
        {
            transform.GetComponent<UILabel>().text = text;
        }
        public virtual void SetText(string text, int size)
        {
            transform.GetComponent<UILabel>().fontSize = size;
            transform.GetComponent<UILabel>().text = text;
        }
        public virtual string GetText()
        {
            return transform.GetComponent<UILabel>().text;
        }

        public virtual void SetColor(Color color)
        {
            transform.GetComponent<UI2DSprite>().color = color;
        }

        public bool IsUIOn()
        {
            int result = -1;

            try
            {
                result = (transform.GetComponent<UIRect>().enabled) ? 1 : 0;
            }
            catch (System.Exception)
            {
                result = (transform.GetComponent<UIWidgetContainer>().enabled) ? 1 : 0;
            }

            if (result == -1) Debug.Log("There is no UIRect OR UIWidgetContainer");

            return (result == 1) ? true : false;
        }
    }
}

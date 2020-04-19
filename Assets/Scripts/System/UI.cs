using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame.UI
{
    public abstract class UI : MonoBehaviour
    {
        /// <summary>
        /// destiPos 위치로 UI를 이동
        /// </summary>
        /// <param name="destiPos"> WorldPos </param>
        protected virtual void Move(Vector3 destiPos)
        {
            destiPos = Camera.main.WorldToViewportPoint(destiPos);

            //화면을 넘어가지 않도록 함
            float x = Mathf.Clamp(destiPos.x, 0.05f, 0.95f);
            float y = Mathf.Clamp(destiPos.y, 0.05f, 0.95f);

            destiPos.Set(x, y, 0);
            transform.position = destiPos;
        } 
    }
}

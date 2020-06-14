using UnityEngine;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class SendMessageToManager : MonoBehaviour
    {
        private UIManager uiManager;
        private StageManager stageManager;

        void Start()
        {
            Transform manager = GameObject.Find("Manager").transform;
            uiManager = manager.GetComponent<UIManager>();
            stageManager = manager.GetComponent<StageManager>();
        }

        public void OffIndicator(string uiName)
        {
            uiManager.SetIndicator(uiName, null);
        }

        public void UpdateHPBar(float ratio)
        {
            StartCoroutine(uiManager.UpdateHPBar(ratio, 0));
        }
    }
}

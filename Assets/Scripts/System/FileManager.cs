using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class FileManager : MonoBehaviour
    {

        private Dictionary<string, string> audioDic = new Dictionary<string, string>();

        // Start is called before the first frame update
        void Start()
        {
            //나중에 파일에서 읽어올 것
            //audioDic.Add("HAND", "someAudioFile");
            audioDic.Add("CAN", "metalPot3");
            audioDic.Add("CHEESE", "bookClose");
            audioDic.Add("SMOKE", "Smoke Grenade");
        }

        public string GetAudioFileName(string key)
        {
            string result = "";

            try { result = audioDic[key]; }
            catch (System.Exception) { result = "NULL"; }
            return result;
        }
    }
}

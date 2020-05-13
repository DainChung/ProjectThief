using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class FileManager : MonoBehaviour
    {
        private float _audioVolume;
        private Dictionary<string, string> audioDic = new Dictionary<string, string>();

        public AudioMixer audioSettings;
        public float audioVolume { get { return _audioVolume; } }

        // Start is called before the first frame update
        void Start()
        {
            float.TryParse(FileIO.DataIO.Read("SoundSetting.data", 0), out _audioVolume);
            audioSettings.SetFloat("AllVolume", _audioVolume);
            GetComponent<UIManager>().SetUISliderValue("Window_Setting", "Sound Slider", (_audioVolume + 80)/100);

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

        public void SetSound(float value)
        {
            audioSettings.SetFloat("AllVolume", -80 + value * 100);
            audioSettings.GetFloat("AllVolume", out _audioVolume);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"> true == MaxVolume, false == MinVolume</param>
        public void SetSoundByBool(bool enable)
        {
            float value = enable ? 20 : -80;
            audioSettings.SetFloat("AllVolume", value);
            audioSettings.GetFloat("AllVolume", out _audioVolume);
        }

        public void SaveSoundSetting()
        {
            FileIO.DataIO.Write("SoundSetting.data", _audioVolume.ToString(), 0);
        }
    }
}

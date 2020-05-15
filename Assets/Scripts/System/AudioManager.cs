using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class AudioManager : MonoBehaviour
    {
        private Dictionary<string, int> audioNameDic = new Dictionary<string, int>();

        public AudioClip[] audioClips;
        public AudioSource audioSource;

        void Start()
        {
            for (int i = 0; i < audioClips.Length; i++)
                audioNameDic.Add(audioClips[i].name, i);
        }

        public void PlayAudio(string name)
        {
            try
            {
                if (audioSource.clip.name != name) audioSource.clip = audioClips[audioNameDic[name]];
            }
            catch (System.Exception) { audioSource.clip = audioClips[audioNameDic[name]]; }

            if (name == "Hit") audioSource.time = 0.55f;
            else audioSource.time = 0f;

            audioSource.Play();
        }

        public void PlayAudioClipDirect(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        public void EnableAudio(bool enable)
        {
            audioSource.volume = (enable ? 1 : 0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;
using Com.MyCompany.MyGame.GameSystem;

namespace Com.MyCompany.MyGame
{
    public class Weapon : MonoBehaviour
    {
        protected new AudioSource audio;
        protected string audioFileName;
        protected Rigidbody rb;
        protected float time;
        protected WeaponCode _code;

        protected void SetCode(WeaponCode input)
        {
            _code = input;

            audio = GetComponent<AudioSource>();
            //code에 따라 재생할 Audio파일 찾아서 재생, "NULL"이면 재생 안 함
            audioFileName = GameObject.FindGameObjectWithTag("Manager").GetComponent<FileManager>().GetAudioFileName(input.ToString());
            if (audioFileName != "NULL") audio.clip = Resources.Load(string.Format("{0}{1}", FilePaths.AudioPath, audioFileName)) as AudioClip;
            else audio.enabled = false;
        }

        protected void PlayAudio()
        {
            try
            {
                if (audio.clip.name == "Smoke Grenade") audio.time = 0.5f;
                audio.Play();
            }
            catch (System.Exception) { }
        }
    }
}

﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class SoundControl : MonoBehaviour {

    public enum SoundType { BGM, SFX }
    public SoundType soundType;

    private void OnEnable()
    {
        //PlayerPrefs.DeleteAll();  // Cleanse playerprefs

        int soundPref;

        switch (soundType)
        {
            case SoundType.BGM:
                if (PlayerPrefs.HasKey("bgmPref"))
                {
                    soundPref = PlayerPrefs.GetInt("bgmPref");
                }
                else
                {
                    PlayerPrefs.SetInt("bgmPref", 1);
                    break;
                }

                if (soundPref == 0)
                {
                    AudioManager.Instance.ToggleBGM();  // BGM off
                }
                else
                {
                }
                break;
            case SoundType.SFX:
                if (PlayerPrefs.HasKey("sfxPref"))
                {
                    soundPref = PlayerPrefs.GetInt("sfxPref");                   
                }
                else
                {
                    PlayerPrefs.SetInt("sfxPref", 1);
                    break;
                }

                if (soundPref == 0)
                {
                    AudioManager.Instance.ToggleSFX();  // SFX off
                }
                else
                {
                }
                break;
        }
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    switch (soundType)
    //    {
    //        case SoundType.BGM:
    //            AudioManager.Instance.ToggleBGM();
    //            PlayerPrefs.SetInt("bgmPref", System.Convert.ToInt32(!muteImage.enabled));
    //            Debug.Log(string.Format("BGM soundPref changed to: {0}", PlayerPrefs.GetInt("bgmPref")));
    //            break;
    //        case SoundType.SFX:
    //            AudioManager.Instance.ToggleSFX();
    //            PlayerPrefs.SetInt("sfxPref", System.Convert.ToInt32(!muteImage.enabled));
    //            Debug.Log(string.Format("SFX soundPref changed to: {0}", PlayerPrefs.GetInt("sfxPref")));
    //            break;
    //        default:
    //            break;
    //    }
    //}
}
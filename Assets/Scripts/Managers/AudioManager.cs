#if UNITY_EDITOR
using System.ComponentModel;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioManager : Singleton<AudioManager> {

    // Set to protected to prevent calling constructor
    protected AudioManager() { }

    AudioMixer m_MainMix;
    AudioSource m_BGM;
    AudioSource m_AMBIENCE;
    AudioSource m_SFX;

    void Awake()
    {
        m_MainMix = Resources.Load("AudioMixers/MainMix", typeof(AudioMixer)) as AudioMixer;

        m_BGM = gameObject.AddComponent<AudioSource>();
        m_BGM.outputAudioMixerGroup = m_MainMix.FindMatchingGroups("BGM")[0];
        m_BGM.mute = System.Convert.ToUInt16(PlayerPrefs.GetInt("bgmPref")) == 0 ? true : false;
        m_BGM.playOnAwake = true;
        m_BGM.loop = true;

        m_AMBIENCE = gameObject.AddComponent<AudioSource>();
        m_AMBIENCE.outputAudioMixerGroup = m_MainMix.FindMatchingGroups("BGM")[0];
        m_AMBIENCE.mute = System.Convert.ToUInt16(PlayerPrefs.GetInt("bgmPref")) == 0 ? true : false;
        m_AMBIENCE.playOnAwake = true;
        m_AMBIENCE.loop = true;

        m_SFX = gameObject.AddComponent<AudioSource>();
        m_SFX.outputAudioMixerGroup = m_MainMix.FindMatchingGroups("SFX")[0];
        m_SFX.mute = System.Convert.ToUInt16(PlayerPrefs.GetInt("sfxPref")) == 0 ? true : false;
        m_SFX.playOnAwake = false;
        m_SFX.loop = false;
    }

    /// <summary>
    /// [BGM] Plays a looping audio clip.
    /// </summary>
    /// <param name="clip">[AudioClip] Clip to be played.</param>
    /// <param name="volume">[float] Volume of SFX</param>
    public void PlayBGM(AudioClip clip, float volume = 1.0f, bool playAmbience = false)
    {
        m_BGM.mute = System.Convert.ToUInt16(PlayerPrefs.GetInt("bgmPref")) == 0 ? true : false;
        m_BGM.clip = clip;
        m_BGM.volume = volume;


        if (!m_BGM.isPlaying)
        {
            m_BGM.Play();
            if (playAmbience)
                m_AMBIENCE.Play();
        }
    }

    /// <summary>
    /// [BGM] Plays a looping audio clip along with an ambience track
    /// </summary>
    /// <param name="bgmClip">[AudioClip] BGM Clip to be played.</param>
    /// <param name="ambienceClip">[AudioClip] Ambience Clip to be played.</param>
    /// <param name="volume">[float] Volume of SFX</param>
    /// <param name="playAmbience">[bool] Play ambience alongside BGM</param>
    public void PlayBGM(AudioClip bgmClip, AudioClip ambienceClip, float volume = 1.0f)
    {
        m_BGM.mute = System.Convert.ToUInt16(PlayerPrefs.GetInt("bgmPref")) == 0 ? true : false;
        m_BGM.clip = bgmClip;
        m_BGM.volume = volume;

        m_AMBIENCE.mute = m_BGM.mute;
        m_AMBIENCE.clip = ambienceClip;
        m_AMBIENCE.volume = volume;

        if (!m_BGM.isPlaying)
        {
            m_BGM.Play();
            m_AMBIENCE.Play();
        }
    }

    /// <summary>
    /// [SFX] Plays a single audio clip.
    /// </summary>
    /// <param name="clip">[AudioClip] Clip to be played.</param>
    /// <param name="pitch">[float] Initial pitch of SFX (+- 0.5f variation)</param>
    /// <param name="volume">[float] Volume of SFX</param>
    public void PlaySFX(AudioClip clip = null, float pitch = 1.0f, float volume = 1.0f)
    {
        if (clip == null)
        {
            //Debug.Log("No AudioClip parameter, returning null...");
            return;
        }
 
        m_SFX.mute = System.Convert.ToUInt16(PlayerPrefs.GetInt("sfxPref")) == 0 ? true : false;
        m_SFX.pitch = UnityEngine.Random.Range(pitch - 0.05f, pitch + 0.05f);
        m_SFX.volume = volume;

        m_SFX.PlayOneShot(clip);       
    }

    /// <summary>
    /// [SFX] Plays a random audio clip.
    /// </summary>
    /// <param name="clips">[AudioClip[]] Array of sound effects</param>
    public void RandomizeSFX(params AudioClip[] clips)
    {
        int randomIndex = UnityEngine.Random.Range(0, clips.Length);
        float randomPitch = UnityEngine.Random.Range(1.0f - 0.05f, 1.0f + 0.05f);

        m_SFX.pitch = randomPitch;
        var clip = clips[randomIndex];
        m_SFX.PlayOneShot(clip);
    }

    /// <summary>
    /// [SFX] Plays a random audio clip with specified pitch.
    /// </summary>
    /// <param name="clips">[AudioClip[]] Array of sound effects</param>
    public void RandomizeSFX(float pitch, params AudioClip[] clips)
    {
        int randomIndex = UnityEngine.Random.Range(0, clips.Length);

        m_SFX.pitch = pitch;
        m_SFX.clip = clips[randomIndex];
        m_SFX.Play();
    }

    public void ToggleBGM()
    {
        m_BGM.mute = !m_BGM.mute;        
        m_AMBIENCE.mute = !m_AMBIENCE.mute;
    }

    public void ToggleSFX()
    {
        m_SFX.mute = !m_SFX.mute;
    }

    public void MuteBGM(bool value)
    {
        m_BGM.mute = value;
        m_AMBIENCE.mute = value;
        PlayerPrefs.SetInt("bgmPref", value == true ? 0 : 1);
    }

    public void MuteSFX(bool value)
    {
        m_SFX.mute = value;
        PlayerPrefs.SetInt("sfxPref", value == true ? 0 : 1);
    }

    public void MuteAmbience(bool value)
    {
        m_AMBIENCE.mute = value;
    }
}

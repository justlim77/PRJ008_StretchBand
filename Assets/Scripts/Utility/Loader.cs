using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour
{
    [Header("Manager Prefabs")]
    public GameObject AudioManagerPrefab;
    public GameObject AudioDatabasePrefab;
    public GameObject KinectManagerPrefab;

    [Header("Scene Configuration")]
    public bool showHandCursor = false;
    public bool allowHandClicks = false;
    public bool controlMouseCursor = false;
    public bool controlMouseDrag = false;
    public bool showMouseCursor = false;

    [Header("Play Background Music")]
    public bool muteBGM = false;
    public bool muteSFX = false;
    public bool muteAmbience = true;
    public AudioClip bgmClip;
    public AudioClip ambienceClip;

    // Use this for initialization
    void Awake ()
    {
        InitializeManagers();

        AudioManager.Instance.MuteBGM(muteBGM);
        AudioManager.Instance.PlayBGM(bgmClip, ambienceClip);
        AudioManager.Instance.MuteAmbience(muteAmbience);
        AudioManager.Instance.MuteSFX(muteSFX);

        Cursor.visible = showMouseCursor;
    }
	
	bool InitializeManagers()
    {
        if (AudioManager.Instance == null)
        {
            GameObject audioManager = Instantiate(AudioManagerPrefab);
            audioManager.name = AudioManagerPrefab.name;
            Debug.LogWarning("AudioManager not found. Creating instance of AudioManager.");
        }

        if(AudioDatabase.Instance == null)
        {
            GameObject audioDatabase = Instantiate(AudioDatabasePrefab);
            audioDatabase.name = AudioDatabasePrefab.name;
            Debug.LogWarning("AudioDatabase not found. Creating instance of AudioDatabase.");
        }

        if (KinectManager.Instance == null)
        {
            GameObject kinectManager = Instantiate(KinectManagerPrefab);
            kinectManager.name = KinectManagerPrefab.name;
            Debug.LogWarning("KinectManager not found. Creating instance of KinectManager.");
            DontDestroyOnLoad(kinectManager);

            InteractionManager interactionManager = InteractionManager.Instance;
            if (interactionManager != null)
            {
                interactionManager.showHandCursor = showHandCursor;
                interactionManager.allowHandClicks = allowHandClicks;
                interactionManager.controlMouseCursor = controlMouseCursor;
                interactionManager.controlMouseDrag = controlMouseDrag;

            }
        }

        //if (InteractionManager.Instance == null)
        //{
        //    GameObject kinectManager = Instantiate(KinectManagerPrefab);
        //    kinectManager.name = KinectManagerPrefab.name;
        //    Debug.LogWarning("InteractionManager not found. Creating instance of InteractionManager.");
        //}

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.showHandCursor = showHandCursor;
            InteractionManager.Instance.allowHandClicks = allowHandClicks;
            InteractionManager.Instance.controlMouseCursor = controlMouseCursor;
            InteractionManager.Instance.controlMouseDrag = controlMouseDrag;
        }

        return true;
    }
}

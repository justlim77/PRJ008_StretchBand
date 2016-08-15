using UnityEngine;
using System.Collections;

public enum SoundType
{
    Background,
    Ambience,
    GameWin,
    Pickup,
    LevelSelected,
    UserDetected,
    UserLost
}

public class AudioDatabase : MonoBehaviour
{
    protected AudioDatabase() { }
    public static AudioDatabase Instance { get; private set; }

    [Header("Background")]
    public AudioClip bgm;

    [Header("Ambience")]
    public AudioClip ambience;

    [Header("Misc")]
    public AudioClip levelSelected;
    public AudioClip[] pickup;    
    public AudioClip userDetected;
    public AudioClip userLost;
    public AudioClip gameWin;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public AudioClip GetClip(SoundType soundType)
    {
        AudioClip clip = null;

        switch (soundType)
        {
            case SoundType.Ambience:
                clip = ambience;
                break;
            case SoundType.Background:
                clip = bgm;
                break;
            case SoundType.LevelSelected:
                clip = levelSelected;
                break;
            case SoundType.Pickup:
                clip = pickup[Random.Range(0,pickup.Length)];
                break;
            case SoundType.UserDetected:
                clip = userDetected;
                break;
            case SoundType.UserLost:
                clip = userLost;
                break;
            case SoundType.GameWin:
                clip = gameWin;
                break;
        }

        return clip;
    }
}

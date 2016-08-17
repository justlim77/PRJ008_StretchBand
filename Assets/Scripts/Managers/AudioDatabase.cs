using UnityEngine;
using System.Collections;

public enum SoundType
{
    Ambience,
    Background,
    BoostWindowActivated,
    Chirp,
    GameWin,
    LevelSelected,
    Pickup,
    SpeedUp,
    UserDetected,
    UserLost
}

public enum BirdSound
{
    Chirp,
    Flapping
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
    public AudioClip boostWindowActivated;
    public AudioClip speedUp;

    [Header("Bird")]
    public AudioClip chirp;
    public AudioClip flapping;

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
            case SoundType.BoostWindowActivated:
                clip = boostWindowActivated;
                break;
            case SoundType.LevelSelected:
                clip = levelSelected;
                break;
            case SoundType.Pickup:
                clip = pickup[Random.Range(0,pickup.Length)];
                break;
            case SoundType.SpeedUp:
                clip = speedUp;
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

    public AudioClip GetClip(BirdSound birdSound)
    {
        AudioClip clip = null;

        switch (birdSound)
        {
            case BirdSound.Chirp:
                clip = chirp;
                break;
            case BirdSound.Flapping:
                clip = flapping;
                break;
        }

        return clip;
    }
}

﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,
    Pregame,
    Playing,
    End,
    Postgame
}

public class GameStateChangedEventArgs : EventArgs
{
    public GameState GameState;
    public float DistanceToTravel;
    public float LandingForwardBuffer;
    public float GameDuration;
    public int BoostCount;
    public int BerriesCollected;
    public float TimeTaken;
}

public class TimerChangedEventArgs : EventArgs
{
    public float Time;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int BerriesRequiredToBoost = 10;

    #region Events and Delegates
    public delegate void GameStateChangedEventHandler(object sender, GameStateChangedEventArgs e);
    public static event GameStateChangedEventHandler GameStateChanged;

    public delegate void TimerChangedEventHandler(object sender, TimerChangedEventArgs e);
    public static event TimerChangedEventHandler TimerChanged;
    #endregion

    [Header("Game Info")]
    public GameState GameState;
    public float GameDuration = 60.0f;
    public float DistanceToTravel = 100.0f;
    public float LandingForwardBuffer = 5.0f;

    int boostBerries;
    int totalBerries { get; set; }
    int boostCount  { get; set; }
    float elapsedTime { get; set; }
    float timeTaken { get; set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        SetState(GameState.Pregame);
    }

    void OnEnable()
    {
        //Bird.DistanceChanged += Bird_DistanceChanged;
        Bird.FruitAmountChanged += Bird_FruitAmountChanged;
        Bird.BoostStateChanged += Bird_BoostStateChanged;
        FlightGestureListener.OnPrimaryUserLost += FlightGestureListener_OnPrimaryUserLost;

    }

    private void FlightGestureListener_OnPrimaryUserLost(string obj)
    {
        //if (GameState == GameState.Postgame || GameState == GameState.End)
            SceneManager.LoadScene(0);
        //else 
        //    SetState(GameState.Pregame);

    }

    private void Bird_FruitAmountChanged(object sender, FruitAmountChangedEventArgs e)
    {
        totalBerries = e.TotalBerries;
    }

    void OnDisable()
    {
        Bird.DistanceChanged -= Bird_DistanceChanged;
        Bird.FruitAmountChanged -= Bird_FruitAmountChanged;
        Bird.BoostStateChanged -= Bird_BoostStateChanged;
        FlightGestureListener.OnPrimaryUserLost -= FlightGestureListener_OnPrimaryUserLost;
    }

    private void Bird_BoostStateChanged(object sender, BoostStateChangedEventArgs e)
    {
        switch (e.BoostState)
        {
            case BoostState.Boosting:
                boostCount++;
                break;
        }
    }

    private void Bird_DistanceChanged(object sender, float distance)
    {
        if (distance >= DistanceToTravel)
        {
            Bird.DistanceChanged -= Bird_DistanceChanged;
            SetState(GameState.End);
        }
    }

    bool timerStart = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetState(GameState.Pregame);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        if (timerStart)
        {
            //ElapsedTime = Time.time - startTime;
            elapsedTime -= Time.deltaTime;
            elapsedTime = Mathf.Clamp(elapsedTime, 0.0f, GameDuration);
            timeTaken += Time.deltaTime;
            OnTimerChanged();
        }    
    }

    public void OnGameStateChanged(GameState state)
    {
        if (GameStateChanged != null)   // If there are listeners
        {
            GameStateChanged(this, new GameStateChangedEventArgs()
            {
                GameState = GameState,
                GameDuration = GameDuration,
                DistanceToTravel = DistanceToTravel,
                LandingForwardBuffer = LandingForwardBuffer,
                BoostCount = boostCount,
                BerriesCollected = totalBerries,
                TimeTaken = timeTaken
            });
        }
    }

    public void OnTimerChanged()
    {
        if (TimerChanged != null)
        {
            TimerChanged(this, new TimerChangedEventArgs() { Time = elapsedTime });
        }
    }

    public void SetState(GameState state)
    {
        if (GameState == state)
            return;

        GameState = state;
        OnGameStateChanged(state);
        Debug.Log("Game state set to " + GameState.ToString());

        switch (state)
        {
            case GameState.Pregame:
                Initialize();
                break;
            case GameState.Playing:
                StartTimer();
                break;
            case GameState.End:
                StopTimer();
                break;
            case GameState.Postgame:
                AudioManager.Instance.PlayOneShot(AudioDatabase.Instance.GetClip(SoundType.GameWin));
                break;
        }
    }

    float startTime;
    void StartTimer()
    {
        timerStart = true;
        startTime = Time.time;
    }

    void ResetTimer()
    {
        timerStart = false;
        elapsedTime = GameDuration;
        timeTaken = 0;
    }

    void StopTimer()
    {
        timerStart = false;
    }

    bool Initialize()
    {
        Bird.DistanceChanged += Bird_DistanceChanged;
        StopTimer();
        ResetTimer();
        boostCount = 0;
        boostBerries = 0;
        totalBerries = 0;
        return true;
    }
}

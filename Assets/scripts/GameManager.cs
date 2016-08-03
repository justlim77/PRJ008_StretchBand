using UnityEngine;
using System.Collections;
using System;

public enum GameState
{
    None,
    Pregame,
    Playing,
    End
}

public class GameStateChangedEventArgs : EventArgs
{
    public GameState GameState;
    public float DistanceToTravel;
    public float GameDuration;
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

    [Header("Bird")]
    public Bird Bird;
    public BirdHouse BirdHouse;
    public float BoostWindow = 5.0f;

    int _BoostBerries;
    public int TotalBerries
    { get; private set; }

    public float ElapsedTime { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        InteractionManager manager = InteractionManager.Instance;
        manager.controlMouseCursor = false;
        manager.allowHandClicks = false;

        SetState(GameState.Pregame);
    }

    void OnEnable()
    {
        Bird.DistanceChanged += Bird_DistanceChanged;
    }
    void OnDisable()
    {
        Bird.DistanceChanged -= Bird_DistanceChanged;
    }

    private void Bird_DistanceChanged(object sender, float distance)
    {
        if (distance >= DistanceToTravel)
        {
            SetState(GameState.End);
        }
    }

    bool updateStats = false;
    bool timerStart = false;
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    SetState(GameState.Playing);
        //}

        if (Input.GetKeyDown(KeyCode.R))
        {
            SetState(GameState.Pregame);
        }

        if (timerStart)
        {
            //ElapsedTime = Time.time - startTime;
            ElapsedTime -= Time.deltaTime;
            ElapsedTime = Mathf.Clamp(ElapsedTime, 0.0f, GameDuration);
            OnTimerChanged();
        }

        if (updateStats)
        {
            UpdateStats();
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
                DistanceToTravel = DistanceToTravel
            });
        }
    }

    public void OnTimerChanged()
    {
        if (TimerChanged != null)
        {
            TimerChanged(this, new TimerChangedEventArgs() { Time = ElapsedTime });
        }
    }

    void UpdateStats()
    {
        //ArduinoUI.Instance.UpdateDistance(Bird.Distance);
        //ArduinoUI.Instance.UpdateTimer(ElapsedTime);
    }

    public void SetState(GameState state)
    {
        if (GameState == state)
            return;

        GameState = state;
        OnGameStateChanged(state);

        switch (state)
        {
            case GameState.Pregame:
                ResetTimer();
                updateStats = false;
                UpdateStats();
                BirdHouse.Distance = DistanceToTravel + Bird.LandingForwardBuffer;
                break;
            case GameState.Playing:
                StartTimer();
                Bird.SetInMotion(true);
                updateStats = true;
                break;
            case GameState.End:
                StopTimer();
                updateStats = false;
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
        ElapsedTime = GameDuration;
    }

    void StopTimer()
    {
        timerStart = false;
    }
}

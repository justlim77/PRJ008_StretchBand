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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Info")]
    public GameState GameState;
    public string PreGameMessage;
    public string EndGameMessage;

    [Header("Bird")]
    public Bird Bird;
    public float DistanceToTravel = 100;
    public BirdHouse BirdHouse;
    public float LandingBuffer = 5.0f;


    public float ElapsedTime { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        SetState(GameState.Pregame);
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
            ElapsedTime = Time.time - startTime;
        }

        if (updateStats)
        {
            UpdateStats();
        }

        // Check game win
        CheckGameWin();      
    }

    void UpdateStats()
    {
        ArduinoUI.Instance.UpdateDistance(Bird.Distance, DistanceToTravel);
        ArduinoUI.Instance.UpdateTimer(ElapsedTime);
        ArduinoUI.Instance.UpdateOutput();
    }

    void CheckGameWin()
    {
        if (Bird.Distance >= DistanceToTravel)
        {
            SetState(GameState.End);
        }
    }

    public void SetState(GameState state)
    {
        if (GameState == state)
            return;

        GameState = state;

        switch (state)
        {
            case GameState.Pregame:
                ResetTimer();
                ArduinoUI.Instance.UpdateMessage(PreGameMessage);
                Bird.Initialize();
                updateStats = false;
                UpdateStats();
                TileManager.Instance.Initialize();
                BirdHouse.Distance = DistanceToTravel + LandingBuffer;
                break;
            case GameState.Playing:
                StartTimer();
                ArduinoUI.Instance.UpdateMessage("");
                Bird.SetInMotion(true);
                updateStats = true;
                break;
            case GameState.End:
                ArduinoUI.Instance.UpdateMessage(EndGameMessage);
                StopTimer();
                updateStats = false;
                Bird.Land();
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
        ElapsedTime = 0;
    }

    void StopTimer()
    {
        timerStart = false;
    }
}

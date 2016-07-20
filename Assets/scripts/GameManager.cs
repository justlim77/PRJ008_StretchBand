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
    public string pregameMessage;
    public string endgameMessage;

    public Bird bird;
    public float distanceToTravel = 100;

    public static GameManager Instance { get; private set; }
    public GameState gameState;

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetState(GameState.Playing);
        }

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
        ArduinoUI.Instance.UpdateDistance(bird.Distance, distanceToTravel);
        ArduinoUI.Instance.UpdateTimer(ElapsedTime);
        ArduinoUI.Instance.UpdateOutput();
    }

    void CheckGameWin()
    {
        if (bird.Distance >= distanceToTravel)
        {
            SetState(GameState.End);
        }
    }

    public void SetState(GameState state)
    {
        if (gameState == state)
            return;

        gameState = state;

        switch (state)
        {
            case GameState.Pregame:
                ResetTimer();
                ArduinoUI.Instance.UpdateMessage(pregameMessage);
                bird.Reset();
                updateStats = false;
                UpdateStats();
                TileManager.Instance.Reset();
                break;
            case GameState.Playing:
                StartTimer();
                ArduinoUI.Instance.UpdateMessage("");
                bird.SetInMotion(true);
                updateStats = true;
                break;
            case GameState.End:
                ArduinoUI.Instance.UpdateMessage(endgameMessage);
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
        ElapsedTime = 0;
    }

    void StopTimer()
    {
        timerStart = false;
    }
}

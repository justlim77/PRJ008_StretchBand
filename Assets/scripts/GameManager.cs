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
    public static int BerriesRequiredToBoost = 10;

    [Header("Game Info")]
    public GameState GameState;
    public float GameDuration = 60.0f;
    public float DistanceToTravel = 100.0f;

    [TextArea]
    public string PreGameMessage;
    [TextArea]
    public string EndGameMessage;

    [Header("Bird")]
    public Bird Bird;
    public BirdHouse BirdHouse;
    public float BoostWindow = 5.0f;

    int _BoostBerries;
    int _TotalBerries;

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
                //TileManager.Instance.Initialize();
                BirdHouse.Distance = DistanceToTravel + Bird.LandingForwardBuffer;
                break;
            case GameState.Playing:
                StartTimer();
                ArduinoUI.Instance.ClearMessage();
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
        //ElapsedTime = 0;
        ElapsedTime = GameDuration;
    }

    void StopTimer()
    {
        timerStart = false;
    }

    bool _IsBoosting = false;
    public void AddBerry()
    {
        _BoostBerries++;
        _TotalBerries++;

        float _BoostProgress = Mathf.Clamp01(_BoostBerries * 0.1f);
        Bird.BoostBar.UpdateProgress(_BoostProgress);

        if (_BoostBerries >= BerriesRequiredToBoost)
        {
            if (!_IsBoosting)
            {
                _IsBoosting = true;
                StartCoroutine(EnableBoostWindow());
            }
        }
    }

    public void ResetBerries()
    {
        _BoostBerries = 0;
        Bird.BoostBar.UpdateProgress(0);
    }

    IEnumerator EnableBoostWindow()
    {
        Bird.CanBoost = true;

        float boostDuration = BoostWindow;
        //while (boostDuration > 0 && Bird.GetAnimation() != AnimationState.Glide)
        //{
        //    boostDuration -= Time.deltaTime;
        //    ArduinoUI.Instance.UpdateMessage(string.Format("ENERGY UP!\nPULL BAND TO GLIDE ({0:F0})", boostDuration));
        //    yield return null;
        //}
        ArduinoUI.Instance.UpdateMessage(string.Format("ENERGY UP!\nPULL BAND TO GLIDE!"));

        yield return new WaitForSeconds(BoostWindow);
        ArduinoUI.Instance.ClearMessage();
        Bird.CanBoost = false;

        ResetBerries();

        _IsBoosting = false;
    }
}

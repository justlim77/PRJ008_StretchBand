using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System;
using System.IO.Ports;
using System.Collections.Generic;

public class ArduinoUI : MonoBehaviour
{
    public static ArduinoUI Instance { get; private set; }

    [Header("Labels")]
    public Text FruitLabel;
    public Text OutputLabel;
    public Text DistanceLabel;
    public Text TimerLabel;
    public Text MessageLabel;
    public Image MessageBackground;

    [Header("Sliders")]
    public Slider DistanceSlider;

    public BoostBar BoostBar;

    [Header("Messages")]
    [TextArea]
    public string PregameMessage;
    [TextArea]
    public string PostgameMessage;

    [Header("UI Animators")]
    public Animator FruitLabelAnimator;

    [Header("Canvas")]
    [SerializeField] Canvas _MainCanvas;
    [SerializeField] Animator _CanvasAnimator;

    [Header("Connection Canvas")]
    public KeyCode toggleConnectionCanvasKey = KeyCode.BackQuote;
    public GameObject connectionCanvas;
    public Button refreshButton;
    public Button connectButton;
    public Button disconnectButton;
    public Dropdown portDropdown;

    [Header("Band Output Variables")]
    public int force;
    public int count;

    float _DistanceToTravel = 0f;
    int _TotalBerries = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

	void Start ()
    {
        //portDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(); });
        //refreshButton.onClick.AddListener(delegate { RefreshPortList(); });
        //connectButton.onClick.AddListener(delegate { Open(); });
        //disconnectButton.onClick.AddListener(delegate { Close(); });

        //UpdatePortDropdown();
    }

    void OnEnable()
    {
        ArduinoConnector.OutputReceived += ArduinoConnector_OutputReceived;
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        GameManager.TimerChanged += GameManager_TimerChanged;
        Bird.FruitAmountChanged += Bird_FruitAmountChanged;
        Bird.BoostStateChanged += Bird_BoostStateChanged;
        Bird.AnimationStateChanged += Bird_AnimationStateChanged;
        Bird.DistanceChanged += Bird_DistanceChanged;
    }

    private void Bird_DistanceChanged(object sender, float distance)
    {
        UpdateDistance(distance);
    }

    private void Bird_AnimationStateChanged(object sender, AnimationStateChangedEventArgs e)
    {
        switch (e.AnimationState)
        {           
            case AnimationState.Takeoff:
                BoostBar.ShowBar(true);
                break;
            case AnimationState.Landing:
                BoostBar.ResetColor();      // Revert bar color
                BoostBar.ShowBar(false);    // Hide bar
                break;
            case AnimationState.Fly:
                BoostBar.ResetColor();
                break;
            case AnimationState.Glide:
                //ClearMessage();
                break;
        }
    }

    private void Bird_BoostStateChanged(object sender, BoostStateChangedEventArgs e)
    {
        switch (e.BoostState)
        {
            case BoostState.Ignition:
                UpdateMessage("Energy Up!\nPull band to boost!", 5);
                break;
            case BoostState.Boosting:
                UpdateMessage("Good Job!", 1);
                break;
            case BoostState.Cancelled:
                //UpdateMessage();
                break;
        }
    }

    private void GameManager_TimerChanged(object sender, TimerChangedEventArgs e)
    {
        UpdateTimer(e.Time);
    }

    void OnDisable()
    {
        ArduinoConnector.OutputReceived -= ArduinoConnector_OutputReceived;
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        GameManager.TimerChanged -= GameManager_TimerChanged;    
        Bird.FruitAmountChanged -= Bird_FruitAmountChanged;
        Bird.BoostStateChanged -= Bird_BoostStateChanged;
        Bird.DistanceChanged -= Bird_DistanceChanged;
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        switch (e.GameState)
        {
            case GameState.Pregame:
                UpdateTimer(e.GameDuration);
                UpdateMessage(PregameMessage);
                _DistanceToTravel = e.DistanceToTravel;
                break;
            case GameState.Playing:
                UpdateMessage("Fly home!", 1);
                break;
            case GameState.End:
                UpdateMessage(PostgameMessage);
                break;
        }
    }

    private void ArduinoConnector_OutputReceived(object sender, OutputReceivedEventArgs e)
    {
        force = e.Force;
        count = e.Count;

        OutputLabel.text = string.Format("{0} N", force);
    }

    private void Bird_FruitAmountChanged(object sender, FruitAmountChangedEventArgs e)
    {
        // Animate UI
        string trigger = FruitLabelAnimationState.Collect.ToString().ToLower();
        FruitLabelAnimator.SetTrigger(trigger);
        FruitLabel.text = e.TotalBerries.ToString();

        // Update boost bar progress
        float boostProgress = Mathf.Clamp01(e.BoostBerries * 0.1f);
        BoostBar.UpdateProgress(boostProgress);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleConnectionCanvasKey))
        {
            connectionCanvas.SetActive(!connectionCanvas.activeSelf);
        }
    }

    void OnDropdownValueChanged()
    {
        ArduinoConnector.Instance.SetPort(portDropdown.options[portDropdown.value].text);
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public void UpdateDistance(float currentDistance)
    {
        //DistanceLabel.text = string.Format("{0:F0} / {1} m", currentDistance, _DistanceToTravel);
        float oldRange = 0 - _DistanceToTravel;
        float newRange = DistanceSlider.maxValue - DistanceSlider.minValue;
        float sliderValue = ((currentDistance - 0) / (_DistanceToTravel - 0) ) * newRange + 0;
        DistanceSlider.value = sliderValue;
    }

    public void UpdateTimer(float time)
    {
        //float min;
        //float sec;

        //min = (int)(time / 60);
        //sec = time % 60;
        //if (sec == 60)
        //{
        //    sec = 0;
        //    min++;
        //}

        //string secString = sec < 10 ? "0" + sec.ToString("F0") : sec.ToString("F0");
        //timerLabel.text = string.Format("{0} m {1} s", min, secString);        

        if (time <= 0.0f)
        {
            time = 0.0f;
            TimerLabel.text = "Time's Up";
        }
        else
        {
            TimerLabel.text = string.Format("{0:F0}", time);
        }
    }

    public void UpdateMessage(string msg = "", float duration = 0)
    {
        MessageLabel.text = msg;

        if (string.IsNullOrEmpty(msg))
        {
            SetAnimation(CanvasAnimationState.HideMessage);
        }
        else
        {
            if(duration > 0)
                StartCoroutine(RunUpdateMessage(msg, duration));
            else
                SetAnimation(CanvasAnimationState.ShowMessage);
        }
    }

    IEnumerator RunUpdateMessage(string msg, float duration)
    {
        SetAnimation(CanvasAnimationState.ShowMessage);

        yield return new WaitForSeconds(duration);

        SetAnimation(CanvasAnimationState.HideMessage);
    }

    public void UpdateFruits(int value)
    {
        FruitLabel.text = string.Format("{0}", value);
    }

    void UpdatePortDropdown()
    {
        portDropdown.ClearOptions();
        RefreshPortList();

        string[] portNames = ArduinoConnector.Instance.PortList;
        foreach (string name in portNames)
            name.ToUpper();
        List<string> portList = new List<string>(portNames);
        portDropdown.AddOptions(portList);
    }

    public void Open()
    {
        ArduinoConnector.Instance.Open();
    }

    public void Close()
    {
        ArduinoConnector.Instance.Close();
    }

    public void RefreshPortList()
    {
        ArduinoConnector.Instance.RefreshPortList();
    }

    void SetAnimation(CanvasAnimationState state)
    {
        string trigger = state.ToString().ToLower();
        _CanvasAnimator.SetTrigger(trigger);
    }
}

public enum CanvasAnimationState
{
    ShowMessage,
    HideMessage
}

public enum FruitLabelAnimationState
{
    Collect
}
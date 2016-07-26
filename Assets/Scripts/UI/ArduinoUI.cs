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
    public Text outputLabel;
    public Text distanceLabel;
    public Text timerLabel;
    public Text messageLabel;

    [Header("Sliders")]
    public Slider distanceSlider;

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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

	// Use this for initialization
	void Start ()
    {
        portDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(); });
        refreshButton.onClick.AddListener(delegate { RefreshPortList(); });
        connectButton.onClick.AddListener(delegate { Open(); });
        disconnectButton.onClick.AddListener(delegate { Close(); });

        UpdatePortDropdown();
        OnDropdownValueChanged();
    }

    void OnEnable()
    {
        ArduinoConnector.OutputReceived += OnOutputReceived;
    }

    void OnDisable()
    {
        ArduinoConnector.OutputReceived -= OnOutputReceived;
    }

    private void OnOutputReceived(object sender, OutputReceivedEventArgs e)
    {
        force = e.Force;
        count = e.Count;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleConnectionCanvasKey))
        {
            connectionCanvas.SetActive(!connectionCanvas.activeSelf);
        }

        UpdateOutput();
    }

    void OnDropdownValueChanged()
    {
        ArduinoConnector.Instance.SetPort(portDropdown.options[portDropdown.value].text);
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public void UpdateDistance(float distance, float distanceToTravel)
    {
        distanceLabel.text = string.Format("{0:F0} / {1} m", distance, distanceToTravel);
        float oldRange = 0 - distanceToTravel;
        float newRange = distanceSlider.maxValue - distanceSlider.minValue;
        float sliderValue = ((distance - 0) / (distanceToTravel - 0) ) * newRange + 0;
        distanceSlider.value = sliderValue;
    }

    public void UpdateOutput()
    {
        outputLabel.text = string.Format("{0} N | {1}", force, count);
    }

    public void UpdateTimer(float time)
    {
        float min;
        float sec;

        min = (int)(time / 60);
        sec = time % 60;
        if (sec == 60)
        {
            sec = 0;
            min++;
        }

        string secString = sec < 10 ? "0" + sec.ToString("F0") : sec.ToString("F0");
        timerLabel.text = string.Format("{0} m {1} s", min, secString);
    }

    public void UpdateMessage(string msg)
    {
        messageLabel.text = msg;
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
}

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
    public Text valueLabel;
    public Text distanceLabel;
    public Text timerLabel;
    public Text messageLabel;

    [Header("Connection Canvas")]
    public KeyCode toggleConnectionCanvasKey = KeyCode.BackQuote;
    public GameObject connectionCanvas;
    public Button refreshButton;
    public Button connectButton;
    public Button disconnectButton;
    public Dropdown portDropdown;

    [Header("Band Variables")]
    //public Text outputLabel;
    public int force;
    public int count;

    string outputString;

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
        ArduinoController.Output += OnOutputReceived;
    }

    void OnDisable()
    {
        ArduinoController.Output -= OnOutputReceived;
    }

    private void OnOutputReceived(object sender, object[] args)
    {
        string output = string.Format("Force: {0} N\nCount: {1]", args[0], args[1]);
        outputLabel.text = output;
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

    int strideCount = 0;
    int pForce, pCount;
    public void UpdateOutputLabel(string msg)
    {
        strideCount++;

        outputLabel.text += string.Format("{0}", msg);


        if (strideCount == 4)
        {
            print(msg);
            int.TryParse(outputLabel.text.Substring(0, 2), out force);
            int.TryParse(outputLabel.text.Substring(2, 2), out count);
            if(force != pForce || count != pCount)
                Debug.Log(string.Format("Force: {1}N | Count: {2}", force, count));
            pForce = force; // Cache force
            pCount = count; // Cache count
            strideCount = 0;
            outputLabel.text = "";
        }
    }

    public void UpdateValueLabel(string msg)
    {
        int value;

        outputString += msg;

        if (msg == " ")
        {
            //count = int.TryParse(outputString.)
        }

        bool readable;
        readable = int.TryParse(msg, out value);
        if (readable)
        {
            outputString += msg;
        }
        else
        {
            outputString = "";
        }

        valueLabel.text = string.Format("Count: {0}\nForce: {1}", count, outputString);
    }

    public void UpdateDistance(float distance, float distanceToTravel)
    {
        distanceLabel.text = string.Format("{0:F0} / {1} m", distance, distanceToTravel);
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

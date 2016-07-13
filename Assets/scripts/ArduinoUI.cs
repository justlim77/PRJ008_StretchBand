using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System;

public class ArduinoUI : MonoBehaviour
{
    public static ArduinoUI Instance { get; private set; }
    public Text outputLabel;
    public Text valueLabel;
    public Text distanceLabel;
    public Text timerLabel;
    public Text messageLabel;

    public int force;
    public int count;
    public string outputString;

    GameManager gameManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

	// Use this for initialization
	void Start ()
    {
        gameManager = GameManager.Instance;
    }

    int strideCount = 0;
    public void UpdateOutputLabel(string msg)
    {
        strideCount++;

        outputLabel.text += string.Format("{0}", msg);

        if (strideCount == 4)
        {
            int.TryParse(outputLabel.text.Substring(0, 2), out force);
            int.TryParse(outputLabel.text.Substring(2, 2), out count);
            Debug.Log(string.Format("force {1}, count {2}", force, count));
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

    public void UpdateDistance(float distance)
    {
        distanceLabel.text = string.Format("{0:F0} / {1} m", distance, gameManager.distanceToTravel);
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
}

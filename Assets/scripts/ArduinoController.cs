using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class ArduinoController : MonoBehaviour
{
    public static ArduinoController Instance { get; private set; }
    public ArduinoConnector connector;
    public ArduinoUI UI;
    public int readTimeout = 5000;
    public float asyncReadTimeout = 10f;
    public int force;
    public int forceStrideLength = 2;
    public int count;
    public int countStrideLength = 2;

    private string streamMessage;
    private int totalStrideLength;

    private StringBuilder _sb;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    // Use this for initialization
    void Start()
    {
        _sb = new StringBuilder();
        totalStrideLength = forceStrideLength + countStrideLength;

        //connector.Open();
        //Action<int> testFunc = (int i) =>
        //{
        //    Debug.Log("WAH!!!");
        //};
        //connector.AsynchronousReadFromArduino(testFunc, () => Debug.LogError("Error!"), asyncReadTimeout);
        //StartCoroutine
        //(
        //connector.AsynchronousReadFromArduino
        //((int i) => ProcessStream(Convert.ToChar(i).ToString()),        // Callback
        ////((int i) => Debug.Log(i),                                     // Debug Callback
        //() => Debug.LogError("Error!"),                                 // Error callback
        //asyncReadTimeout                                                // Timeout (seconds)
        //)
        //);
    }

    void Update()
    {
        //int i = connector.ReadByteFromArduino(readTimeout);
        //int i = ArduinoConnector.Output;
        //ProcessStream(Convert.ToChar(i).ToString());
    }

    private int strideCount = 0;
    public void ProcessStream(string msg)
    {
        strideCount++;

        _sb.Append(msg);
        streamMessage = _sb.ToString();
        //Debug.Log(_sb.ToString());
        if (strideCount >= totalStrideLength)
        {
            int.TryParse(streamMessage.Substring(0, forceStrideLength), out force);
            int.TryParse(streamMessage.Substring(countStrideLength, countStrideLength), out count);
            Debug.Log(string.Format("force {0}, count {1}", force, count));
            _sb.Length = strideCount = 0;
        }
    }

    public bool ForceDetected(int threshhold = 0, bool inclusive = false)
    {
        return inclusive ? force >= threshhold : force > threshhold;
    }

    void OnApplicationQuit()
    {
        connector.Close();
    }

    void OnDestroy()
    {
        Instance = null;
    }
}

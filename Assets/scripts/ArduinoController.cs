using UnityEngine;
using System.Collections;
using System;

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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
	// Use this for initialization
	void Start ()
    {
        totalStrideLength = forceStrideLength + countStrideLength;

        connector.Open();
        //Action<int> testFunc = (int i) =>
        //{
        //    Debug.Log("WAH!!!");
        //};
        //connector.AsynchronousReadFromArduino(testFunc, () => Debug.LogError("Error!"), asyncReadTimeout);
        StartCoroutine
        (
        connector.AsynchronousReadFromArduino
        ((int i) => ProcessStream(Convert.ToChar(i).ToString()),        // Callback
        //((int i) => Debug.Log(i),                                     // Debug Callback
        () => Debug.LogError("Error!"),                                 // Error callback
        asyncReadTimeout                                                // Timeout (seconds)
        )
        );
    }

    void Update()
    {
        //connector.ReadByteFromArduino(readTimeout);
    }

    private int strideCount = 0;
    public void ProcessStream(string msg)
    {
        strideCount++;

        streamMessage += msg;

        if (strideCount >= totalStrideLength)
        {
            int.TryParse(streamMessage.Substring(0, forceStrideLength), out force);
            int.TryParse(streamMessage.Substring(countStrideLength, countStrideLength), out count);
            Debug.Log(string.Format("force {0}, count {1}", force, count));
            strideCount = 0;
            streamMessage = "";
        }
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

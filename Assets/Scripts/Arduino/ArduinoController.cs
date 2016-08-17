using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class ArduinoController : MonoBehaviour
{
    public static ArduinoController Instance { get; private set; }

    public ArduinoConnector connector;
    public int readTimeout = 5000;
    public float asyncReadTimeout = 10f;
    public int force;
    public int forceStrideLength = 2;
    public int count;
    public int countStrideLength = 2;

    [SerializeField] string streamMessage;
    private int totalStrideLength;

    private StringBuilder _sb;

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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    // Use this for initialization
    void Start()
    {
        _sb = new StringBuilder(4, 8);
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

    int strideCount = 0;
    int pForce, pCount;
    public void ProcessStream(string msg)
    {
        strideCount++;

        _sb.Append(msg);
        streamMessage = _sb.ToString();
        if (strideCount >= totalStrideLength)
        {
            int.TryParse(streamMessage.Substring(0, forceStrideLength), out force);
            int.TryParse(streamMessage.Substring(countStrideLength, countStrideLength), out count);

            // Only log 
            //if (force != pForce || count != pCount)
            {
                Debug.Log(string.Format("Force: {0} N | Count: {1}", force, count));
                //OnOutputReceived(force, count);
            }

            // Cache new values
            pForce = force;
            pCount = count;

            // Clear for next buffer
            strideCount = 0;
            _sb.Clear();
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

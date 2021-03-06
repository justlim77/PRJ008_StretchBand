﻿/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using ArduinoLibrary;
using UnityEngine.UI;
using System.IO;

public class OutputReceivedEventArgs : EventArgs
{
    public int Force;
    public int Count;
}

public class ArduinoConnector : MonoBehaviour
{
    public static ArduinoConnector Instance { get; private set; }

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string Port = "COM7";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int Baudrate = 9600;
    /* The parity of the serial port. */
    [Tooltip("The parity of the serial port")]
    public Parity Parity = Parity.None;
    /* The parity of the serial port. */
    [Tooltip("The data bits of the serial port")]
    public int DataBits = 8;
    /* The stop bits of the serial port. */
    [Tooltip("The stop bits of the serial port")]
    public StopBits StopBits = StopBits.One;
    /* The read timeout of the serial port. */
    [Tooltip("The read timeout of the serial port")]
    public int ReadTimeout = 5000;
    /* The wait  interval between serial reads (Default is 0.05f) */
    [Tooltip("The wait interval between serial reads (Default is 0.05f)")]
    public float WaitBetweenReads = 0.05f;
    /* Force connection to specified serial port */
    [Tooltip("Force connection to specified serial port")]
    public bool ForceConnection = false;

    public int StrideLength = 4;

    public Text status;

    private BandReadJob _BandReadJob;
    private BandConnectJob _BandConnectJob;
    private Thread _BandReadThread;
    private Thread _BandConnectThread;

    private SerialPort _Stream;
    private WaitForSeconds _WaitForSeconds;
    private bool _IsRunning;
    private int[] _OutputArray, _CachedArray;
    private int _StrideCount = 0;

    public delegate void OutputReceivedEventHandler(object sender, OutputReceivedEventArgs e);
    public static event OutputReceivedEventHandler OutputReceived;

    public delegate void BandFoundReceivedEventHandler(object sender, SerialPort e);
    public static event BandFoundReceivedEventHandler BandFound;

    public static event Action<string> OnBandConnectionLost;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        _WaitForSeconds = new WaitForSeconds(WaitBetweenReads);
    }

    void Start()
    {
        //StartBandConnection();
    }

    void StartBandConnection()
    {
        _OutputArray = new int[strideLength];
        _CachedArray = new int[strideLength];

        //if (ForceConnection)
        //{
        //    string _port = Port;
        //    Open();
        //}

        //ArduinoDeviceManager deviceManager = new ArduinoDeviceManager();
        //foreach (var key in deviceManager.SerialPorts.Values)
        //{
        //    Debug.Log(key.PortName);
        //}

        //foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())
        //{
        //    Debug.Log(string.Format("{0} – {1}", comPort.Name, comPort.Description));
        //}

        //_Stream = new SerialPort();
        //_Stream = TryGetSerialPorts();

        //StartCoroutine(TryConnection());
        //BandConnectJob.BandFound = false;
        //if(_BandConnectJob != null)
        //    _BandConnectJob.Abort();
        Close();
        _BandConnectJob = null;
        _BandConnectJob = new BandConnectJob(ref _OutputArray, ref _CachedArray, StrideLength, ref _Stream);
        _BandConnectJob.Start();
        BandConnectJob.OnBandConnectionEstablished += BandConnectJob_OnBandConnectionEstablished;
    }

    private void BandConnectJob_OnBandConnectionEstablished(string obj, SerialPort stream)
    {
        BandConnectJob.OnBandConnectionEstablished -= BandConnectJob_OnBandConnectionEstablished;
        _Stream = stream;
        _BandConnectJob.Abort();
        Open();
        //BandReadJob.OnInvalidOutputReceived += BandReadJob_OnInvalidOutputReceived;
    }

    private void BandReadJob_OnInvalidOutputReceived(string obj)
    {
        BandConnectJob.BandFound = false;
        Close();
        Start();    // Restart band connection task
        BandReadJob.OnInvalidOutputReceived -= BandReadJob_OnInvalidOutputReceived;
    }

    IEnumerator TryConnection()
    {
        bool result = false;
        int tries = 50;        

        while (result == false && tries > 0)
        {
            tries--;
            result = TryGetSerialPorts(5000f);
            Debug.Log("Attempting to get band serial port... Attempt " + tries);
            yield return new WaitForEndOfFrame();
        }

        if (result == false)
            Debug.Log("Failed to secure band connection.");
        else
            Debug.Log("Band Connection established!");
    }

    public void Open()
    {
        // Opens the serial port
        //_Stream = new SerialPort(Port, Baudrate, Parity, DataBits, StopBits);
        _Stream.ReadTimeout = ReadTimeout;
        Debug.Log(string.Format("Stream initialized with port {0}, baudrate {1}, parity {2}, {3} data bits, stopbits set to {4}, and read timeout at {5}",
            _Stream.PortName, _Stream.BaudRate, _Stream.Parity, _Stream.DataBits, _Stream.StopBits, _Stream.ReadTimeout));
        Debug.Log("Open connection started...");

        _BandReadJob = new BandReadJob(ref _OutputArray, ref _CachedArray, StrideLength, _Stream);
        _BandReadJob.Start();
    }

    bool _errorRaised = false;
    int _cachedStrideCount = 0;
    float _readTimeout = 2;
    void Update()
    {
        // Band read threaded job
        if (_BandReadJob != null)
        {
            if (_BandReadJob.Update())
            {
                _BandReadJob = null;
                _BandReadJob = new BandReadJob(ref _OutputArray, ref _CachedArray, StrideLength, _Stream);
                _BandReadJob.Start();
            }
        }

        if (BandReadJob.BandUpdate)
        {
            BandReadJob.BandUpdate = false;
            OnOutputReceived(_OutputArray);
        }

        // Band connect threaded job
        if (!BandConnectJob.BandFound)
        {
            if (_BandConnectJob != null)
            {
                if (_BandConnectJob.Update())
                {
                    _BandConnectJob = null;
                    _BandConnectJob = new BandConnectJob(ref _OutputArray, ref _CachedArray, StrideLength, ref _Stream);
                    _BandConnectJob.Start();
                }
            }

            if (BandConnectJob.BandUpdate)
            {
                BandConnectJob.BandUpdate = false;
            }
        }

        // Manual band reconnection
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (OnBandConnectionLost != null)
                OnBandConnectionLost("Searching for band");
            BandConnectJob.BandFound = false;
            StartBandConnection();
        }

        //// Check data lost during band reading
        //else if (BandConnectJob.BandFound)
        //{
        //    if (_BandReadJob != null)
        //    {
        //        _cachedStrideCount = BandReadJob.StrideCount;
        //        _readTimeout -= Time.deltaTime;

        //        if (_readTimeout <= 0)
        //        {
        //            Debug.Log(string.Format("BandStrideCount {0} vs CachedStride {1}", BandReadJob.StrideCount, _cachedStrideCount));
        //            if (BandReadJob.StrideCount == _cachedStrideCount)
        //            {
        //                _readTimeout = 2;
        //                if (!_errorRaised)
        //                {
        //                    _errorRaised = true;
        //                    BandReadJob_OnInvalidOutputReceived("Invalid Output Received");
        //                }
        //            }
        //        }
        //    }
        //}
    }

    public void Close()
    {
        Debug.Log("Close connection started...");

        if (_Stream != null)
        {
            _Stream.Close();
            Debug.Log("Stream closed.");
        }

        _IsRunning = false;    // stop listening thread

        if(_BandReadThread != null)
            _BandReadThread.Join(3000);   // wait for listening thread to terminate (max. 500ms)

        if(_BandReadJob != null)
            _BandReadJob.Abort();

        if (_BandConnectThread != null)
            _BandConnectThread.Join(3000);   // wait for listening thread to terminate (max. 500ms)

        if (_BandConnectJob != null)
            _BandConnectJob.Abort();

        Debug.Log("Close connection completed.");
    }

    public string[] PortList { get; private set; }
    public void RefreshPortList()
    {
        PortList = SerialPort.GetPortNames();
    }

    //Not available in Mono 2.X
    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        int value = sp.ReadByte();
        Debug.Log(value);
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        _Stream.WriteLine(message);
        _Stream.BaseStream.Flush();
    }
    public string ReadStringFromArduino(int timeout = 0)
    {
        _Stream.ReadTimeout = timeout;
        try
        {
            return _Stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
    public int ReadByteFromArduino(int timeout = 0)
    {
        _Stream.ReadTimeout = timeout;
        try
        {
            return Convert.ToInt32(_Stream.ReadByte());
        }
        catch (TimeoutException)
        {
            return 0;
        }
    }
    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            // A single read attempt
            try
            {
                dataString = _Stream.ReadLine();
                //dataString = stream.ReadExisting();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            }
            else
                yield return _WaitForSeconds;

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }
    public IEnumerator AsynchronousReadFromArduino(Action<int> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        int dataString = 0;

        do
        {
            // A single read attempt
            try
            {
                dataString = _Stream.ReadByte();
                _Stream.BaseStream.Flush();
            }
            catch (TimeoutException)
            {
                dataString = 0;
            }

            if (dataString != 0)
            {
                callback(dataString);
                yield return null;
            }
            else
                yield return _WaitForSeconds;

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }

    public static int Output;
    int strideCount = 0;
    public int strideLength = 4;
    StringBuilder _sb = new StringBuilder(4, 8);
    public static bool FlagUpdate = false;
    int[] outputArray = new int[4];
    int[] cachedArray = new int[] { 0, 0, 0, 0 };
    void ReadByte()
    {
        print("Thread running");
        try
        {
            while (_IsRunning)
            {
                outputArray[strideCount] = _Stream.ReadByte();
                strideCount++;
                if (strideCount >= strideLength)
                {                    
                    print(string.Format("Output: {0} {1} {2} {3} | Cached: {4} {5} {6} {7}", outputArray[0], outputArray[1], outputArray[2], outputArray[3],
                        cachedArray[0], cachedArray[1], cachedArray[2], cachedArray[3]));
                    // Compare cached array to new array
                    if (!outputArray.SequenceEqual(cachedArray))
                    {
                        OnOutputReceived(outputArray);
                        FlagUpdate = true;
                    }

                    strideCount = 0;
                }
                //ArduinoController.Instance.ProcessStream(Convert.ToChar(Output).ToString());
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    string outputString = "";
    int force, count;
    private void OnOutputReceived(int[] output)
    {
        if (OutputReceived != null)
        {
            for (int i = 0; i < output.Length; ++i)
                _sb.Append(Convert.ToChar(output[i]));
            outputString = _sb.ToString();
            int.TryParse(outputString.Substring(0, 2), out force);
            int.TryParse(outputString.Substring(2, 2), out count);

            OutputReceived(this, new OutputReceivedEventArgs() { Force = force, Count = count });

            //outputArray.CopyTo(cachedArray, 0);
            _sb.Clear();
        }
    }

    protected void OnBandFound()
    {
        if (BandFound != null)
            BandFound(this, _Stream);
    }

    public void SetPort(string portName)
    {
        Port = portName.ToUpper();
    }

    void OnDestroy()
    {
        Instance = null;
    }

    SerialPort TryGetSerialPorts()
    {
        SerialPort com = new SerialPort();
        foreach (string s in SerialPort.GetPortNames())
        {
            com.Close(); // To handle the exception, in case the port isn't found and then they try again...

            bool portfound = false;
            com.PortName = s;
            com.BaudRate = 38400;
            com.BaudRate = 9600;
            try
            {
                com.Open();
                Debug.Log("Trying port: " + s + "\r");
            }
            catch (IOException c)
            {
                Debug.Log("Invalid Port" + "\r");
            }
            catch (InvalidOperationException c1)
            {
                Debug.Log("Invalid Port" + "\r");
            }
            catch (ArgumentNullException c2)
            {
                // System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c2);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (TimeoutException c3)
            {
                //  System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c3);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (UnauthorizedAccessException c4)
            {
                //System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (ArgumentOutOfRangeException c5)
            {
                //System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c5);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (ArgumentException c2)
            {
                //System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c2);
                Debug.Log("Invalid Port" + "\r");
            }
            if (!portfound)
            {
                if (com.IsOpen) // Port has been opened properly...
                {
                    com.ReadTimeout = ReadTimeout; // 500 millisecond timeout...
                    Debug.Log("Attemption to open port " + com.PortName + "\r");
                    try
                    {
                        Debug.Log("Waiting for a response from controller: " + com.PortName + "\r");
                        //string comms = com.ReadLine();

                        //_Stream = com;
                        //_BandReadJob = new BandReadJob(ref _OutputArray, ref _CachedArray, StrideLength, _Stream);
                        //_BandReadJob.Start();
                        int comms = com.ReadByte();
                        Debug.Log("Reading From Port " + com.PortName + "\r");
                        Debug.Log("Value output: " + Convert.ToChar(comms));
                        //if (comms.Substring(0, 1) == "A") // We have found the arduino!
                        if (comms != 0)
                        {
                            _OutputArray[0] = Convert.ToChar(comms);
                            BandReadJob.StrideCount++;

                            Debug.Log(s + com.PortName + " Opened Successfully!" + "\r");
                            //com.Write("a"); // Sends 0x74 to the arduino letting it know that we are connected!
                            //com.ReadTimeout = ReadTimeout;
                            //com.Write("a");
                            Debug.Log("Port " + com.PortName + " Opened Successfully!" + "\r");
                            Debug.Log(com.BaudRate);
                            Debug.Log(com.PortName);

                            //Port = _Stream.PortName;
                            //Baudrate = _Stream.BaudRate;
                            //Parity = _Stream.Parity;
                            //DataBits = _Stream.DataBits;
                            //StopBits = _Stream.StopBits;
                            Port = com.PortName;
                            Baudrate = com.BaudRate;
                            Parity = com.Parity;
                            DataBits = com.DataBits;
                            StopBits = com.StopBits;

                            _Stream = com;
                            Open();

                            return com;

                        }
                        else
                        {
                            Debug.Log("Port Not Found! Please cycle controller power and try again" + "\r");
                            _BandReadJob.Abort();
                            com.Close();
                        }
                    }
                    catch (Exception e1)
                    {
                        Debug.Log("Incorrect Port! Trying again..." + e1);
                        com.Close();
                    }
                }
            }
        }
        return com;
    }

    bool TryGetSerialPorts(float timeout = 5000f)
    {
        SerialPort com = new SerialPort();
        foreach (string s in SerialPort.GetPortNames())
        {
            com.Close(); // To handle the exception, in case the port isn't found and then they try again...

            bool portfound = false;
            com.PortName = s;
            //com.BaudRate = 38400;
            com.BaudRate = 9600;
            try
            {
                com.Open();
                Debug.Log("Trying port: " + s + "\r");
            }
            catch (IOException c)
            {
                Debug.Log("Invalid Port" + "\r");
            }
            catch (InvalidOperationException c1)
            {
                Debug.Log("Invalid Port" + "\r");
            }
            catch (ArgumentNullException c2)
            {
                // System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c2);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (TimeoutException c3)
            {
                //  System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c3);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (UnauthorizedAccessException c4)
            {
                //System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (ArgumentOutOfRangeException c5)
            {
                //System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c5);
                Debug.Log("Invalid Port" + "\r");
            }
            catch (ArgumentException c2)
            {
                //System.Windows.Forms.MessageBox.Show("Sorry, Exception Occured - " + c2);
                Debug.Log("Invalid Port" + "\r");
            }
            if (!portfound)
            {
                if (com.IsOpen) // Port has been opened properly...
                {
                    com.ReadTimeout = ReadTimeout; // 500 millisecond timeout...
                    Debug.Log("Attemption to open port " + com.PortName + "\r");
                    try
                    {
                        Debug.Log("Waiting for a response from controller: " + com.PortName + "\r");
                        //string comms = com.ReadLine();

                        //_Stream = com;
                        //_BandReadJob = new BandReadJob(ref _OutputArray, ref _CachedArray, StrideLength, _Stream);
                        //_BandReadJob.Start();
                        int comms = com.ReadByte();
                        Debug.Log("Reading From Port " + com.PortName + "\r");
                        Debug.Log("Value output: " + Convert.ToChar(comms));
                        //if (comms.Substring(0, 1) == "A") // We have found the arduino!
                        if (comms != 0)
                        {
                            _OutputArray[0] = Convert.ToChar(comms);
                            BandReadJob.StrideCount++;

                            Debug.Log(s + com.PortName + " Opened Successfully!" + "\r");
                            //com.Write("a"); // Sends 0x74 to the arduino letting it know that we are connected!
                            //com.ReadTimeout = ReadTimeout;
                            //com.Write("a");
                            Debug.Log("Port " + com.PortName + " Opened Successfully!" + "\r");
                            Debug.Log(com.BaudRate);
                            Debug.Log(com.PortName);

                            //Port = _Stream.PortName;
                            //Baudrate = _Stream.BaudRate;
                            //Parity = _Stream.Parity;
                            //DataBits = _Stream.DataBits;
                            //StopBits = _Stream.StopBits;
                            Port = com.PortName;
                            Baudrate = com.BaudRate;
                            Parity = com.Parity;
                            DataBits = com.DataBits;
                            StopBits = com.StopBits;

                            _Stream = com;
                            Open();

                            return true;

                        }
                        else
                        {
                            Debug.Log("Port Not Found! Please cycle controller power and try again" + "\r");
                            _BandReadJob.Abort();
                            com.Close();
                        }
                    }
                    catch (Exception e1)
                    {
                        Debug.Log("Incorrect Port! Trying again..." + e1);
                        com.Close();
                    }
                }
            }
        }
        return false;
    }

    void OnApplicationQuit()
    {
        Close();
    }
}
/* ArduinoConnector by Alan Zucconi
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

    private BandReadJob _BandReadJob;

    private Thread _Thread;
    private SerialPort _Stream;
    private WaitForSeconds _WaitForSeconds;
    private bool _IsRunning;
    private int[] _OutputArray, _CachedArray;
    private int _StrideCount = 0;

    public delegate void OutputReceivedEventHandler(object sender, OutputReceivedEventArgs e);
    public static event OutputReceivedEventHandler OutputReceived;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        _WaitForSeconds = new WaitForSeconds(WaitBetweenReads);
    }

    void Start()
    {
        _OutputArray = new int[strideLength];
        _CachedArray = new int[strideLength];

        if (ForceConnection)
        {
            string _port = Port;
            Open();
        }
    }

    public void Open()
    {
        // Opens the serial port
        _Stream = new SerialPort(Port, Baudrate, Parity, DataBits, StopBits);
        Debug.Log(string.Format("Stream initialized with port {0}, baudrate {1}, parity {2}, {3} data bits, and stopbits set to {4}",
            Port, Baudrate, Parity, DataBits, StopBits));
        Debug.Log("Open connection started...");

        //stream.DataReceived += (x,e) =>
        //{
        //    if(e.EventType == SerialData.Eof)
        //    {
        //        Debug.Log("nothing here");
        //    }
        //    else
        //    {
        //        SerialPort sp = (SerialPort)x;
        //        string indata = sp.ReadExisting();
        //        Debug.Log(indata);
        //    }
        //};

        try
        {
            if (_Stream != null)
            {
                _Stream.Open();
                _Stream.ReadTimeout = ReadTimeout;
                Debug.Log("Port opened!");
                //this.stream.DataReceived += DataReceivedHandler;
                //this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            }
            else
            {
                if (_Stream.IsOpen)
                {
                    print("Port is already open.");
                }
                else
                {
                    print("Port is null.");
                }
            }

            Debug.Log("Open connection completed.");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not open serial port: " + e.Message);
        }

        //_running = true;
        //ThreadStart ts = new ThreadStart(ReadByte);
        //_thread = new Thread(ts);
        //_thread.Start();

        _BandReadJob = new BandReadJob(ref _OutputArray, ref _CachedArray, StrideLength, _Stream);
        _BandReadJob.Start();
    }

    void Update()
    {
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

        if(_Thread != null)
            _Thread.Join(500);   // wait for listening thread to terminate (max. 500ms)

        _BandReadJob.Abort();

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
    public void OnOutputReceived(int[] output)
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

    public void SetPort(string portName)
    {
        Port = portName.ToUpper();
    }

    void OnDestroy()
    {
        Instance = null;
    }
}
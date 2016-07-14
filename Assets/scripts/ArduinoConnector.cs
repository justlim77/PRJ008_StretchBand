/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System.Net.Sockets;

public class ArduinoConnector : MonoBehaviour
{
    public static ArduinoConnector Instance { get; private set; }

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string port = "COM7";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 9600;
    /* The parity of the serial port. */
    [Tooltip("The parity of the serial port")]
    public Parity parity = Parity.None;
    /* The parity of the serial port. */
    [Tooltip("The data bits of the serial port")]
    public int dataBits = 1;
    /* The stop bits of the serial port. */
    [Tooltip("The stop bits of the serial port")]
    public StopBits stopBits = StopBits.One;
    /* The read timeout of the serial port. */
    [Tooltip("The read timeout of the serial port")]
    public int readTimeout = 5000;
    /* The wait  interval between serial reads (Default is 0.05f) */
    [Tooltip("The wait interval between serial reads (Default is 0.05f)")]
    public float waitBetweenReads = 0.05f;

    private Thread _thread;
    private SerialPort _stream;
    private WaitForSeconds _waitForSeconds;
    private bool _running;
    private string[] _ports;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        _waitForSeconds = new WaitForSeconds(waitBetweenReads);
    }

    public void Open()
    {
        // Opens the serial port
        _stream = new SerialPort(port, baudrate, parity, dataBits, stopBits);
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
            if (_stream != null)
            {
                _stream.Open();
                _stream.ReadTimeout = readTimeout;
                Debug.Log("Port opened!");
                //this.stream.DataReceived += DataReceivedHandler;
                //this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            }
            else
            {
                if (_stream.IsOpen)
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

        _running = true;
        ThreadStart ts = new ThreadStart(ReadByte);
        _thread = new Thread(ts);
        _thread.Start();
    }

    public void Close()
    {
        Debug.Log("Close connection started...");

        if (_stream != null)
        {
            _stream.Close();
            Debug.Log("Stream closed.");
        }

        _running = false;    // stop listening thread

        if(_thread != null)
            _thread.Join(500);   // wait for listening thread to terminate (max. 500ms)

        Debug.Log("Close connection completed.");
    }

    public void RefreshPortList()
    {
        PortList = SerialPort.GetPortNames();
    }

    public string[] PortList { get; private set; }

    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        int value = sp.ReadByte();
        Debug.Log(value);
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        _stream.WriteLine(message);
        _stream.BaseStream.Flush();
    }

    public string ReadStringFromArduino(int timeout = 0)
    {
        _stream.ReadTimeout = timeout;
        try
        {
            return _stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    public int ReadByteFromArduino(int timeout = 0)
    {
        _stream.ReadTimeout = timeout;
        try
        {
            return Convert.ToInt32(_stream.ReadByte());
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
                dataString = _stream.ReadLine();
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
                yield return _waitForSeconds;

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
                dataString = _stream.ReadByte();
                _stream.BaseStream.Flush();
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
                yield return _waitForSeconds;

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }


    public static int Output;
    void ReadByte()
    {
        print("Thread running");
        try
        {
            while (_running)
            {
                Output = _stream.ReadByte();
                ArduinoController.Instance.ProcessStream(Convert.ToChar(Output).ToString());
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void SetPort(string portName)
    {
        port = portName.ToUpper();
    }

    void OnDestroy()
    {
        Instance = null;
    }
}
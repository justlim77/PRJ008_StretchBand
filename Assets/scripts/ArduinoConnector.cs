/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

public class ArduinoConnector : MonoBehaviour
{

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string port = "COM7";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 9600;
    /* The wait  interval between serial reads (Default is 0.05f) */
    [Tooltip("The wait interval between serial reads (Default is 0.05f)")]
    public float waitBetweenReads = 0.05f;

    private SerialPort stream;
    private WaitForSeconds waitForSeconds;

    void Awake()
    {
        waitForSeconds = new WaitForSeconds(waitBetweenReads);
    }

    public void Open()
    {
        // Opens the serial port
        stream = new SerialPort(port, baudrate);
        stream.ReadTimeout = 5000;
        
       // stream.re
        stream.DataReceived += (x,e) =>
        {
            if(e.EventType == SerialData.Eof)
            {
                Debug.Log("nothing here");
            }
            else
            {
                SerialPort sp = (SerialPort)x;
                string indata = sp.ReadExisting();
                Debug.Log(indata);
            }
        };

        stream.Open();
        //this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        stream.WriteLine(message);
        stream.BaseStream.Flush();
    }

    public string ReadStringFromArduino(int timeout = 0)
    {
        stream.ReadTimeout = timeout;
        try
        {
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    public int ReadByteFromArduino(int timeout = 0)
    {
        stream.ReadTimeout = timeout;
        try
        {
            return Convert.ToInt32(stream.ReadByte());
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
                dataString = stream.ReadLine();
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
                yield return new WaitForSeconds(0.05f);

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
                dataString = stream.ReadByte();
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
                yield return waitForSeconds;

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }

    public void Close()
    {
        stream.Close();
    }
}
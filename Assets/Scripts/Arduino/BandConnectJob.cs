using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.IO;
using System;

public class BandConnectJob : ThreadedJob
{
    private int[] _OutputArray = new int[] { 0, 0, 0, 0 };
    private int[] _CachedArray = new int[] { 0, 0, 0, 0 };
    private SerialPort _Stream = null;
    private int _StrideLength = 0;
    public static int StrideCount = 0;

    private bool m_bandFound = false;
    private int m_readTimeout = 3000;
    public static bool BandFound = false;
    private int m_readResult = 0;

    public BandConnectJob() { }
    public BandConnectJob(ref int[] output, ref int[] cached, int strideLength, ref SerialPort stream)
    {
        _OutputArray = output;
        _CachedArray = cached;
        _StrideLength = strideLength;
        _Stream = stream;
    }

    public static event Action<string, SerialPort> OnBandConnectionEstablished;

    protected void BandConnectionEstablished()
    {
        if (OnBandConnectionEstablished != null)
            OnBandConnectionEstablished("Band connection established", _Stream);
    }

    protected override void ThreadedFunction()
    {
        if (BandFound)
            return;

        // Read serial data - don't use any Unity API
        _Stream = new SerialPort();
        foreach (string s in SerialPort.GetPortNames())
        {
            _Stream.Close(); // To handle the exception, in case the port isn't found and then they try again...

            bool portfound = false;
            _Stream.PortName = s;
            //com.BaudRate = 38400;
            _Stream.BaudRate = 9600;
            try
            {
                _Stream.Open();
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
                if (_Stream.IsOpen) // Port has been opened properly...
                {
                    _Stream.ReadTimeout = m_readTimeout; // 500 millisecond timeout...
                    Debug.Log("Attemption to open port " + _Stream.PortName + "\r");
                    try
                    {
                        Debug.Log("Waiting for a response from controller: " + _Stream.PortName + "\r");
                        //string comms = com.ReadLine();

                        //_Stream = com;
                        //_BandReadJob = new BandReadJob(ref _OutputArray, ref _CachedArray, StrideLength, _Stream);
                        //_BandReadJob.Start();
                        int comms = _Stream.ReadByte();
                        Debug.Log("Reading From Port " + _Stream.PortName + "\r");
                        Debug.Log("Value output: " + Convert.ToChar(comms));
                        m_readResult = comms;
                        //if (comms.Substring(0, 1) == "A") // We have found the arduino!
                        
                    }
                    catch (Exception e1)
                    {
                        Debug.Log("Incorrect Port! Trying again..." + e1);
                        _Stream.Close();
                    }
                }
            }
        }
    }

    public static bool BandUpdate = false;
    protected override void OnFinished()
    {
        //_StrideCount++;

        if (m_readResult != 0)
        {
            BandFound = true;

            _OutputArray[0] = Convert.ToChar(m_readResult);
            BandReadJob.StrideCount++;

            Debug.Log(_Stream.PortName + " Opened Successfully!" + "\r");
            //com.Write("a"); // Sends 0x74 to the arduino letting it know that we are connected!
            //com.ReadTimeout = ReadTimeout;
            //com.Write("a");
            Debug.Log("Port " + _Stream.PortName + " Opened Successfully!" + "\r");
            Debug.Log(_Stream.BaudRate);
            Debug.Log(_Stream.PortName);

            BandConnectionEstablished();        
        }
        else
        {
            Debug.Log("Port Not Found! Please cycle controller power and try again" + "\r");
            //_BandReadJob.Abort();
            _Stream.Close();
            BandUpdate = true;
        }
    }
}

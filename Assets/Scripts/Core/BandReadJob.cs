using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Linq;

public class BandReadJob : ThreadedJob
{
    private int[] _OutputArray = new int[] { 0, 0, 0, 0 };
    private int[] _CachedArray = new int[] { 0, 0, 0, 0 };
    private SerialPort _Stream = null;
    private int _StrideLength = 0;
    public static int StrideCount = 0;

    public BandReadJob() { }
    public BandReadJob(ref int[] output, ref int[] cached, int strideLength, SerialPort stream)
    {
        _OutputArray = output;
        _CachedArray = cached;
        _StrideLength = strideLength;
        _Stream = stream;
    }

    protected override void ThreadedFunction()
    {
        // Read serial data - don't use any Unity API
        _OutputArray[StrideCount] = _Stream.ReadByte();
        StrideCount++;
    }

    public static bool BandUpdate = false;
    protected override void OnFinished()
    {
        //_StrideCount++;

        // Executed by main Unity thread when job is completed
        if (StrideCount >= _StrideLength)
        {
            // Compare cached array to new array
            if (!_OutputArray.SequenceEqual(_CachedArray))
            {
                //OnOutputReceived(outputArray);
                BandUpdate = true;
                _OutputArray.CopyTo(_CachedArray, 0);
            }

            //Debug.Log(string.Format("Output: {0} {1} {2} {3} | Cached: {4} {5} {6} {7}", _OutputArray[0], _OutputArray[1], _OutputArray[2], _OutputArray[3],
            //    _CachedArray[0], _CachedArray[1], _CachedArray[2], _CachedArray[3]));

            StrideCount = 0;
        }
    }
}

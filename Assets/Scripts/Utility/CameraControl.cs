using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class CameraControl : MonoBehaviour
{
    public int boostFOV = 85;
    public float smoothTime = 1.0f;

    public KeyCode captureScreenshotKey = KeyCode.F12;
    public int superSize = 4;
    public string screenshotFolderName = "Fly Home";

    float _initialFOV;
    float _targetFOV;

    ScreenCapture screenCapture;

    string _picturesFolderPath;
    string picturesFolderPath
    {
        get
        {
            if (_picturesFolderPath == "")
            {
                _picturesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), screenshotFolderName);
                if (!Directory.Exists(_picturesFolderPath))
                {
                    Directory.CreateDirectory(_picturesFolderPath);
                }
            }
            return _picturesFolderPath;
        }
    }

    void Start()
    {
        _initialFOV = Camera.main.fieldOfView;
        _targetFOV = _initialFOV;
        screenCapture = GetComponent<ScreenCapture>();
    }

    void OnEnable()
    {
        Bird.BoostStateChanged += Bird_BoostStateChanged;
    }

    void OnDisable()
    {
        Bird.BoostStateChanged -= Bird_BoostStateChanged;
    }

    private void Bird_BoostStateChanged(object sender, BoostStateChangedEventArgs e)
    {
        switch (e.BoostState)
        {
            case BoostState.Boosting:
                _targetFOV = boostFOV;
                break;
            case BoostState.Cancelled:
                _targetFOV = _initialFOV;
                break;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // FOV
        if (Camera.main.fieldOfView != _targetFOV)
        {
            Camera.main.fieldOfView = Mathf.SmoothStep(Camera.main.fieldOfView, _targetFOV, smoothTime * Time.deltaTime);
        }

        // Screenshot
        if (Input.GetKeyDown(captureScreenshotKey))
        {
            TakeScreenshot();
        }
	}

    void TakeScreenshot()
    {
        string filePath = string.Format("IMG_{0}.jpg", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        string realPath = System.IO.Path.Combine(picturesFolderPath, filePath);
        screenCapture.SaveScreenshot(CaptureMethod.AppCapture_Asynch, realPath, superSize);
    }
}

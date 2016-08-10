using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public int boostFOV = 85;
    public float smoothTime = 1.0f;

    float _initialFOV;
    float _targetFOV;

    void Start()
    {
        _initialFOV = Camera.main.fieldOfView;
        _targetFOV = _initialFOV;
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
	}
}

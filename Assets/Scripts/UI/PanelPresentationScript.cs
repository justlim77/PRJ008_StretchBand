using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelPresentationScript : MonoBehaviour
{
    public PanelBase MainPanel;
    public PanelBase LevelPanel;

    public bool ControlMouseCursor = false;

    CanvasScaler _Scaler;

    private FlightGestureListener gestureListener;

	// Use this for initialization
	void Start ()
    {
        _Scaler = GetComponent<CanvasScaler>();

        // get the gestures listener
        gestureListener = FlightGestureListener.Instance;

        //MainPanel.AnchoredPosition = Vector2.zero;
        //LevelPanel.AnchoredPosition = new Vector2(_Scaler.referenceResolution.x, 0);
        InteractionManager interactionManager = InteractionManager.Instance;
        interactionManager.controlMouseCursor = ControlMouseCursor;
    }

    // Update is called once per frame
    void Update()
    {
        // dont run Update() if there is no gesture listener
        if (!gestureListener)
            return;

        if (gestureListener)
        {
            if (gestureListener.IsPrimaryUserDetected())
            {
                LevelPanel.SetOrientation(PanelOrientation.Center, _Scaler.referenceResolution);
            }
            else if (gestureListener.IsPrimaryUserLost())
            {
                SlideLevelPanel();
            }
        }
    }

    public void SlideLevelPanel()
    {
        LevelPanel.SetOrientation(PanelOrientation.Left, _Scaler.referenceResolution);
    }
}

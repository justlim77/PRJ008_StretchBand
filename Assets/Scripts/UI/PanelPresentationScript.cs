using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        StartCoroutine(LevelLoaded());
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
                Debug.Log("Primary user detected.");
            }
            else if (gestureListener.IsPrimaryUserLost())
            {
                SlideLevelPanel();
                Debug.Log("Primary user lost.");
            }
        }
    }

    public void SlideLevelPanel()
    {
        LevelPanel.SetOrientation(PanelOrientation.Left, _Scaler.referenceResolution);
    }

    IEnumerator LevelLoaded()
    {
        KinectManager.Instance.refreshAvatarControllers();
        KinectManager.Instance.refreshGestureListeners();
        yield return new WaitForEndOfFrame();
    }
}

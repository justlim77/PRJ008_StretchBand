﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
//using Windows.Kinect;

public class FlightGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    public static event Action<string> OnPrimaryUserLost;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("GUI-Text to display gesture-listener messages and gesture information.")]
    public GUIText gestureInfo;

    // singleton instance of the class
    private static FlightGestureListener instance = null;

    // internal variables to track if progress message has been displayed
    private bool progressDisplayed;
    private float progressGestureTime;

    // whether the needed gesture has been detected or not
    private bool swipeLeft;
    private bool swipeRight;
    private bool swipeUp;

    private bool primaryUserDetected;
    private bool primaryUserLost;

    /// <summary>
    /// Gets the singleton CubeGestureListener instance.
    /// </summary>
    /// <value>The CubeGestureListener instance.</value>
    public static FlightGestureListener Instance
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// Determines whether swipe left is detected.
    /// </summary>
    /// <returns><c>true</c> if swipe left is detected; otherwise, <c>false</c>.</returns>
    public bool IsSwipeLeft()
    {
        if (swipeLeft)
        {
            swipeLeft = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether swipe right is detected.
    /// </summary>
    /// <returns><c>true</c> if swipe right is detected; otherwise, <c>false</c>.</returns>
    public bool IsSwipeRight()
    {
        if (swipeRight)
        {
            swipeRight = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether swipe up is detected.
    /// </summary>
    /// <returns><c>true</c> if swipe up is detected; otherwise, <c>false</c>.</returns>
    public bool IsSwipeUp()
    {
        if (swipeUp)
        {
            swipeUp = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether swipe right is detected.
    /// </summary>
    /// <returns><c>true</c> if swipe right is detected; otherwise, <c>false</c>.</returns>
    public bool IsPrimaryUserDetected()
    {
        if (primaryUserDetected)
        {
            primaryUserDetected = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether swipe right is detected.
    /// </summary>
    /// <returns><c>true</c> if swipe right is detected; otherwise, <c>false</c>.</returns>
    public bool IsPrimaryUserLost()
    {
        if (primaryUserLost)
        {
            primaryUserLost = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    public void UserDetected(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only
        KinectManager manager = KinectManager.Instance;
        if (!manager || (userIndex != playerIndex))
            return;

        AudioManager.Instance.PlaySFX(AudioDatabase.Instance.GetClip(SoundType.UserDetected));

        // detect these user specific gestures
        manager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
        manager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
        manager.DetectGesture(userId, KinectGestures.Gestures.SwipeUp);

        primaryUserDetected = true;

        if (gestureInfo != null)
        {
            gestureInfo.GetComponent<GUIText>().text = "Swipe left, right or up to change the slides.";
        }

        InteractionManager interactionManager = InteractionManager.Instance;
        interactionManager.CalibrationText.text = "USER FOUND";

        Debug.Log("Main user found");
    }

    /// <summary>
    /// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    public void UserLost(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return;

        primaryUserLost = true;

        AudioManager.Instance.PlaySFX(AudioDatabase.Instance.GetClip(SoundType.UserLost));

        if (OnPrimaryUserLost != null)
            OnPrimaryUserLost("Primary user lost");
                
        if (gestureInfo != null)
        {
            gestureInfo.GetComponent<GUIText>().text = string.Empty;
        }

        InteractionManager interactionManager = InteractionManager.Instance;
        interactionManager.CalibrationText.text = "USER LOST";
    }

    /// <summary>
    /// Invoked when a gesture is in progress.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="progress">Gesture progress [0..1]</param>
    /// <param name="joint">Joint type</param>
    /// <param name="screenPos">Normalized viewport position</param>
    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return;

        if ((gesture == KinectGestures.Gestures.ZoomOut || gesture == KinectGestures.Gestures.ZoomIn) && progress > 0.5f)
        {
            if (gestureInfo != null)
            {
                string sGestureText = string.Format("{0} - {1:F0}%", gesture, screenPos.z * 100f);
                gestureInfo.GetComponent<GUIText>().text = sGestureText;

                progressDisplayed = true;
                progressGestureTime = Time.realtimeSinceStartup;
            }
        }
        else if ((gesture == KinectGestures.Gestures.Wheel || gesture == KinectGestures.Gestures.LeanLeft ||
                 gesture == KinectGestures.Gestures.LeanRight) && progress > 0.5f)
        {
            if (gestureInfo != null)
            {
                string sGestureText = string.Format("{0} - {1:F0} degrees", gesture, screenPos.z);
                gestureInfo.GetComponent<GUIText>().text = sGestureText;

                progressDisplayed = true;
                progressGestureTime = Time.realtimeSinceStartup;
            }
        }
        else if (gesture == KinectGestures.Gestures.Run && progress > 0.5f)
        {
            if (gestureInfo != null)
            {
                string sGestureText = string.Format("{0} - progress: {1:F0}%", gesture, progress * 100);
                gestureInfo.GetComponent<GUIText>().text = sGestureText;

                progressDisplayed = true;
                progressGestureTime = Time.realtimeSinceStartup;
            }
        }
    }

    /// <summary>
    /// Invoked if a gesture is completed.
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="joint">Joint type</param>
    /// <param name="screenPos">Normalized viewport position</param>
    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return false;

        if (gestureInfo != null)
        {
            string sGestureText = gesture + " detected";
            gestureInfo.GetComponent<GUIText>().text = sGestureText;
        }

        if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            swipeLeft = true;
            GameManager gm = GameManager.Instance;
            if (gm)
            {
                if(gm.GameState == GameState.Postgame)
                    SceneManager.LoadScene(0);
            } 
        }
        else if (gesture == KinectGestures.Gestures.SwipeRight)
        {
            swipeRight = true;
            GameManager gm = GameManager.Instance;
            if (gm)
            {
                if (gm.GameState == GameState.Postgame)
                    gm.SetState(GameState.Pregame);
            }
        }
        else if (gesture == KinectGestures.Gestures.SwipeUp)
            swipeUp = true;

        return true;
    }

    /// <summary>
    /// Invoked if a gesture is cancelled.
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="joint">Joint type</param>
    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return false;

        if (progressDisplayed)
        {
            progressDisplayed = false;

            if (gestureInfo != null)
            {
                gestureInfo.GetComponent<GUIText>().text = String.Empty;
            }
        }

        return true;
    }


    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
        {
            progressDisplayed = false;
            gestureInfo.GetComponent<GUIText>().text = String.Empty;
            InteractionManager interactionManager = InteractionManager.Instance;
            interactionManager.CalibrationText.text = string.Empty;
            Debug.Log("Forced progress to end.");
        }
    }

}

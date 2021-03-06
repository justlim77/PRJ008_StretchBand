﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;

public class InteractionListener : MonoBehaviour, InteractionListenerInterface
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Text to display the interaction information.")]
	public Text interactionInfo;

	private bool intInfoDisplayed;
	private float intInfoTime;


	public void HandGripDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
	{
		if (userIndex != playerIndex || !isHandInteracting)
			return;

		string sGestureText = string.Format ("{0} Grip detected; Pos: {1}", !isRightHand ? "Left" : "Right", handScreenPos);

        //InteractionManager manager = InteractionManager.Instance;
        //manager.CalibrationText.text = sGestureText;
        //interactionInfo.text = sGestureText;
		//Debug.Log (sGestureText);

		intInfoDisplayed = true;
		intInfoTime = Time.realtimeSinceStartup;
	}

	public void HandReleaseDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
	{
		if (userIndex != playerIndex || !isHandInteracting)
			return;

		string sGestureText = string.Format ("{0} Release detected; Pos: {1}", !isRightHand ? "Left" : "Right", handScreenPos);

        //InteractionManager manager = InteractionManager.Instance;
        //manager.CalibrationText.text = sGestureText;
        //interactionInfo.text = sGestureText;
		//Debug.Log (sGestureText);

		intInfoDisplayed = true;
		intInfoTime = Time.realtimeSinceStartup;
	}

    public bool HandClickDetected(long userId, int userIndex, bool isRightHand, Vector3 handScreenPos)
    {
        if (userIndex != playerIndex)
            return false;

        string sGestureText = string.Format("{0} Click detected; Pos: {1}", !isRightHand ? "Left" : "Right", handScreenPos);

        //InteractionManager manager = InteractionManager.Instance;
        //manager.CalibrationText.text = sGestureText;
        //interactionInfo.text = sGestureText;
		Debug.Log (sGestureText);

		intInfoDisplayed = true;
		intInfoTime = Time.realtimeSinceStartup;

		return true;
	}


	void Update () 
	{
		// clear the info after 2 seconds
		if(intInfoDisplayed && ((Time.realtimeSinceStartup - intInfoTime) > 2f))
		{
			intInfoDisplayed = false;

			if(interactionInfo != null)
			{
                interactionInfo.text = string.Empty;
			}

            //InteractionManager manager = InteractionManager.Instance;
            //manager.CalibrationText.text = string.Empty;
        }
	}
}

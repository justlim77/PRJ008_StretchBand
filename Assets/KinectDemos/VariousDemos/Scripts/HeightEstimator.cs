using UnityEngine;
using System.Collections;

public class HeightEstimator : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("GUI-texture used to display the tracked users on scene background.")]
	public GUITexture backgroundImage;

	[Tooltip("GUI-Text to display status messages.")]
	public GUIText statusText;

	[Tooltip("Estimated user-silhouette height, in meters.")]
	public float userHeight;

	[Tooltip("Estimated user-silhouette width, in meters.")]
	public float userWidth;


	// user bounds in meters
	private float userLeft;
	private float userTop;
	private float userRight;
	private float userBottom;

	// user bounds in depth points
	private Vector2 posLeft, posTop, posRight, posBottom;

	private KinectManager manager;
	private long lastDepthFrameTime;


	void Start () 
	{
		manager = KinectManager.Instance;

		if (manager && manager.IsInitialized ()) 
		{
			if(backgroundImage)
			{
				Vector3 localScale = backgroundImage.transform.localScale;
				localScale.x = (float)manager.GetDepthImageWidth() * (float)Screen.height / ((float)manager.GetDepthImageHeight() * (float)Screen.width);
				localScale.y = -1f;

				backgroundImage.transform.localScale = localScale;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(backgroundImage && backgroundImage.texture == null)
		{
			backgroundImage.texture = manager ? manager.GetUsersLblTex() : null;
		}

		EstimateDepthRect();
	}


	private bool EstimateDepthRect()
	{
		if (!manager || !manager.IsInitialized ())
			return false;
		
		KinectInterop.SensorData sensorData = manager.GetSensorData();

		long userId = manager.GetUserIdByIndex (playerIndex);
		byte bodyIndex = (byte)manager.GetBodyIndexByUserId (userId);

		if (bodyIndex == 255)
			return false;

		if (sensorData.bodyIndexImage != null && sensorData.depthImage != null && 
			sensorData.lastDepthFrameTime != lastDepthFrameTime) 
		{
			lastDepthFrameTime = sensorData.lastDepthFrameTime;

			int depthLength = sensorData.depthImage.Length;
			int depthWidth = sensorData.depthImageWidth;

			posLeft.x = sensorData.depthImageWidth;
			posTop.y = sensorData.depthImageHeight;
			posRight.x = -1;
			posBottom.y = -1;

			for (int i = 0, x = 0, y = 0; i < depthLength; i++) 
			{
				if (sensorData.bodyIndexImage [i] == bodyIndex) 
				{
					if (posLeft.x > x)
						posLeft = new Vector2(x, y);
					if (posTop.y > y)
						posTop = new Vector2(x, y);
					if (posRight.x < x)
						posRight = new Vector2(x, y);
					if (posBottom.y < y)
						posBottom = new Vector2(x, y);
				}

				x++;
				if (x >= depthWidth) 
				{
					x = 0;
					y++;
				}
			}
		}

		bool bFound = (posRight.x >= 0) && (posBottom.y >= 0);

		if (bFound) 
		{
			Vector3 vPosLeft = manager.MapDepthPointToSpaceCoords (posLeft, sensorData.depthImage [(int)posLeft.y * sensorData.depthImageWidth + (int)posLeft.x], true);
			userLeft = vPosLeft.x;

			Vector3 vPosTop = manager.MapDepthPointToSpaceCoords (posTop, sensorData.depthImage [(int)posTop.y * sensorData.depthImageWidth + (int)posTop.x], true);
			userTop = vPosTop.y;

			Vector3 vPosRight = manager.MapDepthPointToSpaceCoords (posRight, sensorData.depthImage [(int)posRight.y * sensorData.depthImageWidth + (int)posRight.x], true);
			userRight = vPosRight.x;

			Vector3 vPosBottom = manager.MapDepthPointToSpaceCoords (posBottom, sensorData.depthImage [(int)posBottom.y * sensorData.depthImageWidth + (int)posBottom.x], true);
			userBottom = vPosBottom.y;

			userHeight = userTop - userBottom;
			userWidth = userRight - userLeft;

			if (statusText) 
			{
				string sUserInfo = string.Format ("User {0} height: {1:F1} m", playerIndex, userHeight);
				statusText.text = sUserInfo;
			}
		} 
		else 
		{
			if (statusText) 
			{
				string sUserInfo = string.Format ("User {0} not found", playerIndex);
				statusText.text = sUserInfo;
			}
		}

		return false;
	}

}

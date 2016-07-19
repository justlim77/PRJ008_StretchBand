using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	public class UGUIFetchAchievements : SASAchieveSDK {
		
		public InputField
			name_field;

		public Button 
			fetch_button;

		public Text
			errorMessage;

		public SASDisplay
			achievement_display_prefab;

		List<SASAchievement>
			all_achievements;

		SASDisplay
			display; 

		//At the start of the scene, fetch all achievements that exist on the server and store them as reference
		void Start()
		{
			name_field.enabled = false;
			name_field.text = string.Empty;
			FetchAllAchievements(SetReady, OnErrorMessage);
		}

		void SetReady(object all)
		{
			all_achievements = (List<SASAchievement>)all;
			name_field.enabled = true;
		}

		void OnErrorMessage(string error)
		{
			if (null != errorMessage)
			{
				errorMessage.text = error;
				Invoke("ClearErrorMessage", 3);
			}
		}
		
		void ClearErrorMessage()
		{
			errorMessage.text = string.Empty;
		}

		// Update is called once per frame
		void Update () {
			fetch_button.interactable = 
				(!(name_field.text == string.Empty));
		}

		public void OnFetch()
		{
			FetchAchievements(name_field.text.Trim(), LoadPrefabs, LoadAnyway, true);
		}

		void LoadAnyway(string error)
		{
			OnErrorMessage(error);
			//send an empty list as the second param to SetupAchievements to disable
			//all of them. If it is null, they will all be enabled
			DisplayContent(new List<SASAchievement>());
		}

		void LoadPrefabs(object data)
		{
			List<SASAchievement> achievements = (List<SASAchievement>) data;
			DisplayContent(achievements);
		}

		void DisplayContent(List<SASAchievement> filter)
		{
			if (null == display)
			{
				display = Instantiate(achievement_display_prefab) as SASDisplay;
				display.transform.SetParent(transform.root, false);
			}
			display.SetupAchievements(all_achievements, filter);
		}
	}
}

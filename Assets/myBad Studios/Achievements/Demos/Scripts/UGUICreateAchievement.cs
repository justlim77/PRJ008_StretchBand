using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MBS {
	public class UGUICreateAchievement : SASAchieveSDK {
		
		public InputField
			name_field,
			descr_field,
			req_field,
			locked_field,
			unlocked_field;

		public Button 
			create_button;

		public Text
			errorMessage;

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
			create_button.interactable = 
				(!(name_field.text == string.Empty || 
			      descr_field.text == string.Empty || 
			      req_field.text == string.Empty || 
			      locked_field.text == string.Empty || 
				   unlocked_field.text == string.Empty));
		}

		public void OnCancel()
		{
			name_field.text = 
			descr_field.text =
			req_field.text =
			locked_field.text =
			unlocked_field.text = string.Empty;
		}

		public void OnCreate()
		{
			CreateNewAchievement(name_field.text.Trim(), 
			                     descr_field.text.Trim(),
			                     req_field.text.Trim(),
			                     unlocked_field.text.Trim(),
			                     locked_field.text.Trim(),
			                     ClearFields,
			                     OnErrorMessage);
		}

		void ClearFields(object nothing)
		{
			OnCancel();
		}
	}
}

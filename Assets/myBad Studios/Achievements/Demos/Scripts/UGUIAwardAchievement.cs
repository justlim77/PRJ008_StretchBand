using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MBS {
	public class UGUIAwardAchievement : SASAchieveSDK {
		
		public InputField
			name_field,
			descr_field;

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
			      descr_field.text == string.Empty));
		}

		public void OnCancel()
		{
			name_field.text = string.Empty;
			descr_field.text = "0";
		}

		public void OnAward()
		{
			int id = 0;
			if (!int.TryParse(descr_field.text.Trim(), out id))
			{
				OnErrorMessage("Award id must be numeric");
				return;
			}
			AwardAchievement(id,name_field.text.Trim(), onSuccess:ClearFields, onFail:OnErrorMessage);
		}

		void ClearFields(object nothing)
		{
			name_field.text = string.Empty;
			descr_field.text = "0";
		}
	}
}

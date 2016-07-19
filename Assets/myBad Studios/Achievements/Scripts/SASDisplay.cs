using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	public class SASDisplay : MonoBehaviour {

		public RectTransform
			content_area;

		public SASView
			view_prefab;

		public void ClearAchievements()
		{
			SASView[] all_views = content_area.GetComponentsInChildren<SASView>();
			if (null != content_area)
				foreach(SASView view in all_views)
					Destroy (view.gameObject);
		}

		public void SetupAchievements(List<SASAchievement> achievements, List<SASAchievement> filter = null)
		{
			ClearAchievements();
			GridLayoutGroup glg = content_area.GetComponent<GridLayoutGroup>();
			content_area.sizeDelta = new Vector2(content_area.sizeDelta.x, achievements.Count * (view_prefab.GetComponent<RectTransform>().sizeDelta.y + glg.spacing.y) );

			foreach(SASAchievement achievement in achievements)
			{
				SASView view = Instantiate(view_prefab);
				view.transform.SetParent(content_area);
				view.name.text = achievement.Name;

				if (null == filter)
				{
					view.icon.sprite = achievement.UnlockedImg;
					view.description.text = achievement.Description;
				} else
				{
					bool available = false;
					foreach(SASAchievement runner in filter)
					{
						if (runner.Name == achievement.Name)
						{
							available = true;
							view.icon.sprite = achievement.UnlockedImg;
							view.description.text = achievement.Description;
						}
					}
					if (!available)
					{
						view.icon.sprite = achievement.LockedImg;
						view.description.text = achievement.Requirements;
					}
				}
			}
		}
	}
}
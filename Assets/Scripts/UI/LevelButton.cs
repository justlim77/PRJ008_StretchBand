using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class LevelButton : Button
{
    [SerializeField] Image _LoadingRing;
    [SerializeField] string _LevelToLoad;

    public float FillAmount
    {
        get { return _LoadingRing.fillAmount; }
        set
        {
            if (_LoadingRing != null)
            {
                _LoadingRing.fillAmount = value;
            }
        }
    }

    public Image LoadingRing
    {
        get { return _LoadingRing; }
        set { _LoadingRing = value; }
    }

    public string LevelToLoad
    {
        get { return _LevelToLoad; }
        set { _LevelToLoad = value; }
    }
}

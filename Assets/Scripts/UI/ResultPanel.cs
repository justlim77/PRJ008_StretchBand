using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    public Text berryCountLabel;
    public Text timerCountLabel;
    public Text boostCountLabel;
    public Camera resultCamera;

    CanvasGroup _canvasGroup;
    CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            return _canvasGroup;
        }
    }

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        switch (e.GameState)
        {
            case GameState.Postgame:
                berryCountLabel.text = string.Format("<size=96>{0}</size> berries collected", e.BerriesCollected);
                timerCountLabel.text = string.Format("<size=96>{0}</size> seconds taken", e.TimeTaken);
                boostCountLabel.text = string.Format("<size=96>{0}</size> boost(s) achieved", e.BoostCount);
                canvasGroup.DOFade(1, 0.5f);
                break;
            case GameState.Pregame:
                canvasGroup.DOFade(0, 0.25f);
                break;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            canvasGroup.DOFade(1, 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            canvasGroup.DOFade(0, 0.25f);
        }
    }
}

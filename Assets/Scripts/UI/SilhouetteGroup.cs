using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SilhouetteGroup : MonoBehaviour
{
    [Header("Movement Prompts")]
    public Image silhouetteUp;
    public Image silhouetteDown;
    public Image silhouetteLeft;
    public Image silhouetteRight;
    public float fadeDuration = 1.0f;

    [Header("Boost Prompts")]
    public Image silhouetteStretch;
    public float lerpDuration = 1.0f;

    float _alpha = 0.0f;

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        Bird.BoostStateChanged += Bird_BoostStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        Bird.BoostStateChanged -= Bird_BoostStateChanged;
    }

    private void Bird_BoostStateChanged(object sender, BoostStateChangedEventArgs e)
    {
        switch (e.BoostState)
        {
            case BoostState.Ignition:
                silhouetteStretch.enabled = true;
                break;
            case BoostState.Boosting:
            case BoostState.Cancelled:
                silhouetteStretch.enabled = false;
                break;
        }
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        switch (e.GameState)
        {
            case GameState.Pregame:
                silhouetteUp.CrossFadeAlpha(1, fadeDuration, true);
                silhouetteDown.CrossFadeAlpha(1, fadeDuration, true);
                silhouetteLeft.CrossFadeAlpha(1, fadeDuration, true);
                silhouetteRight.CrossFadeAlpha(1, fadeDuration, true);
                silhouetteStretch.enabled = false;
                break;
            case GameState.Playing:
                silhouetteUp.CrossFadeAlpha(0, fadeDuration, true);
                silhouetteDown.CrossFadeAlpha(0, fadeDuration, true);
                silhouetteLeft.CrossFadeAlpha(0, fadeDuration, true);
                silhouetteRight.CrossFadeAlpha(0, fadeDuration, true);
                break;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(silhouetteStretch.enabled)
            LerpAlpha();
	}

    void LerpAlpha()
    {
        float lerp = Mathf.PingPong(Time.time, lerpDuration) / lerpDuration;
        _alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, lerp));
        Color color = silhouetteStretch.color;
        color.a = _alpha;
        silhouetteStretch.color = color;
    }
}

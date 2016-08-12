using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class DistanceSlider : MonoBehaviour
{
    public Image backgroundImage;
    public Image fillImage;
    public float fadeDuration = 1.0f;

    public void Fade(FadeType fadeType)
    {
        switch (fadeType)
        {
            case FadeType.In:
                backgroundImage.CrossFadeAlpha(1.0f, fadeDuration, true);
                fillImage.CrossFadeAlpha(1.0f, fadeDuration, true);
                break;
            case FadeType.Out:
                backgroundImage.CrossFadeAlpha(0f, fadeDuration, true);
                fillImage.CrossFadeAlpha(0f, fadeDuration, true);
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BoostBar : MonoBehaviour
{
    Image _image;
    Image image
    {
        get
        {
            if (_image == null)
                _image = GetComponent<Image>();
            return _image;
        }
    }

    public Image ProgressBar;
    public Color BoostColor;
    public float LerpSpeed = 2.0f;
    public float FadeDuration = 1.0f;

    float _TargetProgress = 0.0f;
    Color _OriginalColor;

    void OnEnable()
    {
        Bird.AnimationStateChanged += Bird_AnimationStateChanged;
        Bird.BoostStateChanged += Bird_BoostStateChanged;
    }

    private void Bird_BoostStateChanged(object sender, BoostStateChangedEventArgs e)
    {
        switch (e.BoostState)
        {
            case BoostState.Boosting:
                ShowBar(false);
                break;
            case BoostState.Cancelled:
                ShowBar(true);
                break;
        }
    }

    void OnDisable()
    {
        Bird.AnimationStateChanged -= Bird_AnimationStateChanged;
        Bird.BoostStateChanged -= Bird_BoostStateChanged;
    }

    private void Bird_AnimationStateChanged(object sender, AnimationStateChangedEventArgs e)
    {
        switch (e.AnimationState)
        {
            case AnimationState.Idle:
                Reset();
                break;
            case AnimationState.Takeoff:
                ShowBar(true);
                break;
            case AnimationState.Landing:
                ResetColor();      // Revert bar color
                ShowBar(false);    // Hide bar
                break;
            case AnimationState.Fly:
                ResetColor();
                break;
        }
    }

    void Start()
    {
        _OriginalColor = ProgressBar.color;
    }

    public bool Reset()
    {
        ShowBar(false);
        _TargetProgress = 0;
        ResetColor();
        return true;
    }

    public void ShowBar(bool value)
    {
        float fadeTo = value ? 1 : 0;
        float initialAlpha = value ? 0 : 1;

        image.canvasRenderer.SetAlpha(initialAlpha);
        ProgressBar.canvasRenderer.SetAlpha(initialAlpha);

        image.CrossFadeAlpha(fadeTo, FadeDuration, true);
        ProgressBar.CrossFadeAlpha(fadeTo, FadeDuration, true);
    }

    public void UpdateProgress(float progress)
    {
        _TargetProgress = progress;
    }

    public void SetColor(Color color)
    {
        ProgressBar.CrossFadeColor(color, FadeDuration, true, true);
    }

    public void SetBoostColor()
    {
        ProgressBar.CrossFadeColor(BoostColor, FadeDuration, true, true);
    }

    public void ResetColor()
    {
        ProgressBar.CrossFadeColor(_OriginalColor, FadeDuration, true, true);
    }

    void Update()
    {
        float currentProgress = ProgressBar.fillAmount;
        if (currentProgress != _TargetProgress)
        {
            ProgressBar.fillAmount = Mathf.MoveTowards(currentProgress, _TargetProgress, LerpSpeed * Time.deltaTime);
        }
    }
}

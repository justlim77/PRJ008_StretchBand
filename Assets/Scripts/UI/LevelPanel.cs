using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelPanel : PanelBase
{
    public LevelButton[] LevelButtons;

    public void AsynchronousLoadLevel(LevelButton button)
    {
        AudioManager.Instance.PlayOneShot(AudioDatabase.Instance.GetClip(SoundType.LevelSelected));

        StartCoroutine(AsynchronousLoad(button));
    }

    IEnumerator AsynchronousLoad(LevelButton button)
    {
        // Load scene asynchronously
        yield return null;

        // Disable all buttons
        SetButtonsInteractable(false);

        AsyncOperation ao = SceneManager.LoadSceneAsync(button.LevelToLoad);
        ao.allowSceneActivation = false;

        float progress = 0;
        while (!ao.isDone)
        {
            // [0, 0.9] > [0, 1]
            progress = Mathf.Clamp01(ao.progress / 0.9f);
            Debug.Log("Loading progress :" + (progress * 100) + "%");

            // Loading completed
            if (ao.progress == 0.9f)
            {
                progress = 1.0f;
                button.FillAmount = progress;
                Debug.Log("Loading completed!");
                ao.allowSceneActivation = true;
            }

            button.FillAmount = progress;

            yield return null;
        }
    }

    void SetButtonsInteractable(bool value)
    {
        foreach (var button in LevelButtons)
        {
            button.interactable = value;
        }
    }
}

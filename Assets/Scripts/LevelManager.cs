using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public CanvasGroup LoadingPanel;
    public float FadeSpeed = 0.5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

	// Use this for initialization
	void Start ()
    {
	}

    public void NextScene(CanvasGroup canvasGroup)
    {
        StartCoroutine(AsynchronousLoad(canvasGroup));
    }

    IEnumerator AsynchronousLoad(CanvasGroup cg)
    {
        float target = cg.alpha == 1 ? 0 : 1;
        while (cg.alpha != target)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, target, FadeSpeed * Time.deltaTime);
            Debug.Log(cg.alpha);
            yield return null;
        }
        cg.alpha = target;
        Debug.Log(cg.alpha);

        int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIdx = currentSceneIdx++;

        // Load scene asynchronously
        yield return null;

        AsyncOperation ao = SceneManager.LoadSceneAsync("Forest");
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
                Debug.Log("Loading completed!");
                ao.allowSceneActivation = true;
            }

            yield return null;
        }

    }
}

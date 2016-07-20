using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Signpost : MonoBehaviour
{
    public static Signpost Instance { get; private set; }
    public Text Label;
    public float InitialDistance = 100.0f;

    Vector3 _InitialPos;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

	void Start ()
    {
        _InitialPos = this.transform.position;
        Initialize();
	}

    public void UpdateSign(Vector3 position, float distance)
    {
        transform.position = position;
        Label.text = string.Format("{0} m", distance);
    }

    public void Initialize()
    {
        transform.position = _InitialPos;
        Label.text = string.Format("{0} m", InitialDistance);
    }
}

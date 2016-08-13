using UnityEngine;
using System.Collections;

public class BirdHouse : MonoBehaviour
{
    [SerializeField] Transform _LandingSpot;

    public static BirdHouse Instance { get; private set; }

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        Distance = e.DistanceToTravel + e.LandingForwardBuffer;
    }

    public Vector3 LandingPosition
    {
        get { return _LandingSpot.position; }
    }

    public Vector3 Position
    {
        get
        {
            return this.transform.position;
        }

        set
        {
            this.transform.position = value;
        }
    }

    public float Distance
    {
        set
        {
            this.transform.position = new Vector3(0, 0, value);
        }
    }
}

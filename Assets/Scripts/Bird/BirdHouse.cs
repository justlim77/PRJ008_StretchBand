using UnityEngine;
using System.Collections;

public class BirdHouse : MonoBehaviour
{
    [SerializeField] Transform _LandingSpot;

    // Use this for initialization
    void Start()
    {

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

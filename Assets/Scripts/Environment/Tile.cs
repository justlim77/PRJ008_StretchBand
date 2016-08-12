using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public float TileLength;
    public Signpost[] Signposts;
    public float DistanceInterval = 50.0f;

	void Start ()
    {
	}

    //void OnTriggerExit(Collider other)
    //{
    //    TileManager manager = TileManager.Instance;
    //    Invoke("Deactivate", 3);
    //    manager.Spawn();
    //}

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public bool Initialize()
    {
        Vector3 tilePosition = transform.position;
        foreach (var signpost in Signposts)
        {
            tilePosition.z += DistanceInterval;
            signpost.UpdateSign(tilePosition.z);
        }

        return true;
    }

    public void SetPosition(Vector3 position)
    {
        this.transform.position = position;
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}

using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public float TileLength;
    public Signpost[] Signposts;
    public float DistanceInterval = 50.0f;

    [SerializeField] Collider _Collider;

	// Use this for initialization
	void Start ()
    {
        _Collider.isTrigger = true;
	}

    void OnTriggerExit(Collider other)
    {
        TileManager manager = TileManager.Instance;
        manager.Spawn();
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
}

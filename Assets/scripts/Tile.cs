using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public float TileLength;
    public Signpost MidPost, EndPost;

    Collider _Collider;

	// Use this for initialization
	void Start ()
    {
        _Collider = GetComponent<Collider>();
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
        MidPost.UpdateSign(transform.position.z + (TileLength * 0.5f));
        EndPost.UpdateSign(transform.position.z + TileLength);

        return true;
    }
}

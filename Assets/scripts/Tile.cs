using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public float tileLength;
    Collider col;

	// Use this for initialization
	void Start ()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
	}

    void OnTriggerEnter()
    {
        Signpost post = Signpost.Instance;
        post.UpdateSign()
    }

    void OnTriggerExit()
    {
        TileManager manager = TileManager.Instance;
        manager.Spawn();
        gameObject.SetActive(false);
    }
}

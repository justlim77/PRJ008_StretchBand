using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }
    public Vector3 startPosition;
    public GameObject tilePrefab;
    public List<GameObject> tiles = new List<GameObject>();
    public int tilesToCache = 5;
    public int tilesToSpawn = 3;

    Vector3 _lastTilePos;

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
        _lastTilePos = startPosition;

        for (int i = 0; i < tilesToCache; i++)
        {
            GameObject tile = (GameObject)Instantiate(tilePrefab);
            tile.SetActive(false);
            tiles.Add(tile);
        }

        Spawn(tilesToSpawn);
	}

    GameObject GetTile()
    {
        GameObject retVal = null;

        foreach (GameObject tile in tiles)
        {
            if (tile.activeSelf == false)
            {
                retVal = tile;
                continue;
            }
        }

        if (retVal == null)
        {
            GameObject tile = (GameObject)Instantiate(tilePrefab);
            tiles.Add(tile);
            tile.SetActive(false);
            retVal = tile;
        }

        return retVal;
    }

    bool firstTile = true;
    public void Spawn(int tileAmount = 1)
    {
        for (int i = 0; i < tileAmount; i++)
        {
            GameObject tileObject = GetTile();
            Tile tile = tileObject.GetComponent<Tile>();

            float length = tile.tileLength;

            Vector3 nextTilePos = _lastTilePos;
            nextTilePos.z += length;

            if (firstTile)
            {
                firstTile = false;
                nextTilePos.z -= length;
            }

            
            tileObject.transform.position = nextTilePos;
            tileObject.SetActive(true);

            _lastTilePos = nextTilePos;
        }
    }

    public void Reset()
    {
        foreach (var tile in tiles)
        {
            tile.SetActive(false);
            tile.transform.position = Vector3.zero;
        }
        _lastTilePos = startPosition;
        firstTile = true;
        Spawn(tilesToSpawn);
    }
}

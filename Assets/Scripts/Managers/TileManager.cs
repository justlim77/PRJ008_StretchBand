using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    public GameObject TilePrefab;
    public Vector3 StartPosition;
    public bool Cache = true;
    public int CacheAmount = 1;
    public int InitialSpawnAmount = 2;

    List<GameObject> _Tiles = new List<GameObject>();
    Vector3 _LastTilePosition;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (Cache)
        {
            for (int i = 0; i < CacheAmount; i++)
            {
                AddNewTile();
            }
        }
    }

    void OnDestroy()
    {
        Instance = null;
    }

    GameObject GetTile()
    {
        GameObject retVal = null;

        foreach (GameObject tile in _Tiles)
        {
            if (tile.activeSelf == false)
            {
                retVal = tile;
                return retVal;
            }
        }

        if (retVal == null)
        {
            retVal = AddNewTile();
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

            float length = tile.TileLength;

            Vector3 nextTilePos = _LastTilePosition;
            nextTilePos.z += length;

            if (firstTile)
            {
                firstTile = false;
                nextTilePos.z -= length;
            }

            tile.SetPosition(nextTilePos);
            tile.Initialize();
            tile.SetActive(true);

            _LastTilePosition = nextTilePos;
        }
    }

    GameObject AddNewTile()
    {
        GameObject tile = Instantiate(TilePrefab);
        tile.SetActive(false);
        tile.transform.SetParent(this.transform);
        _Tiles.Add(tile);
        return tile;
    }

    public void Initialize()
    {
        foreach (var tile in _Tiles)
        {
            tile.SetActive(false);
            tile.transform.position = Vector3.zero;
        }
        _LastTilePosition = StartPosition;
        firstTile = true;
        Spawn(InitialSpawnAmount);
    }
}

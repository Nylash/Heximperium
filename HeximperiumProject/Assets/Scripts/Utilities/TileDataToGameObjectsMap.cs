using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileDataToGameObjectsMap
{
    public TileData tileData;
    public List<GameObject> gameObjects;

    public TileDataToGameObjectsMap(TileData t, List<GameObject> g)
    {
        tileData = t;
        gameObjects = g;
    }
}

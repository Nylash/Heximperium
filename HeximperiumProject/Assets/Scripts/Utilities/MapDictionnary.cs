using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDictionnary
{
    public List<Vector2> tiles = new List<Vector2>();
    public List<TileData> data = new List<TileData>();

    public MapDictionnary(List<Vector2> t, List<TileData> d)
    {
        tiles = t;
        data = d;
    }
}
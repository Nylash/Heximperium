using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    [SerializeField] private Biome _biome;
    [SerializeField] private TileData _tileData;
    [SerializeField] private Vector2 _coordinate;

    private Tile[] _neighbors = new Tile[6];
    private bool _revealed;
    private bool _claimed;
    private Border _border;

    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }
    public TileData TileData { get => _tileData;}
    public bool Claimed { get => _claimed;}
    public bool Revealed { get => _revealed;}
    public Biome Biome { get => _biome; set => _biome = value; }
    public Tile[] Neighbors { get => _neighbors;}

    private void Awake()
    {
        MapManager.Instance.event_mapGenerated.AddListener(SearchNeighbors);
    }


    public void ClaimTile()
    {
        _claimed = true;
        _border = Instantiate(Resources.Load<GameObject>("Border"), transform.position, Quaternion.identity).GetComponent<Border>();
    }

    public void CheckBorder()
    {
        _border.CheckBorderVisibility(_neighbors);
    }

    private void ApplyMaterial()
    {
        
        if (_tileData.name == "Water")
        {
            //Material not linked to biome
            GetComponent<Renderer>().material = Resources.Load(_tileData.name) as Material;
            return;
        }
        GetComponent<Renderer>().material = Resources.Load(_biome + "/" + _tileData.name + "_" + _biome) as Material;
    }

    private void SearchNeighbors()
    {
        // Determine the offset based on the row
        int rowOffset = Mathf.Abs((int)Coordinate.y % 2);

        // Define the possible directions for neighbors based on your coordinate system
        Vector2[] directions = new Vector2[]
        {
            new Vector2(rowOffset, 1),   // Top-right
            new Vector2(1, 0),    // Right
            new Vector2(rowOffset, -1),   // Bottom-right
            new Vector2(rowOffset - 1, -1),  // Bottom-left
            new Vector2(-1, 0),   // Left
            new Vector2(rowOffset - 1, 1)   // Top-left
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 neighborCoord = Coordinate + directions[i];

            // Check if the neighbor exists in the dictionary
            if (MapManager.Instance.Tiles.TryGetValue(neighborCoord, out Tile neighborTile))
            {
                _neighbors[i] = neighborTile;
            }
        }
    }

    public bool IsOneNeighborClaimed()
    {
        foreach (Tile tile in _neighbors)
        {
            if(tile != null)
            {
                if (tile.Claimed)
                    return true;
            }
        }
        return false;
    }
}

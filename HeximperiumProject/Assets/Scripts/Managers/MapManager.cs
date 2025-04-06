using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class MapManager : Singleton<MapManager>
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private int _mapRadius;
    [SerializeField] private GameObject _predefinedMap;
    [SerializeField] private GameObject _emptyMap;
    private float _deltaX = 1f;
    private float _deltaZ = 0.91f;
    private Transform _grid;
    private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();

    [HideInInspector] public UnityEvent event_mapGenerated;

    public Dictionary<Vector2, Tile> Tiles { get => _tiles;}

    private void OnEnable()
    {
        if (event_mapGenerated == null)
            event_mapGenerated = new UnityEvent();
    }

    void Start()
    {
        if(_predefinedMap != null)
        {
            GameObject bite = Instantiate(_predefinedMap);
            _grid = bite.transform;

            //Manually get all tiles in dictionnary
            for (int i = 0; i < _grid.childCount; i++)
            {
                Tile tile = _grid.GetChild(i).GetComponent<Tile>();
                _tiles[tile.Coordinate] = tile;
            }
        }
        else
        {
            _grid = Instantiate(_emptyMap).transform;
            GenerateHexagonalGrid();
        }
        
        //Search neighbors for each tiles
        foreach (Tile tile in _tiles.Values)
        {
            tile.SearchNeighbors();
        }

        event_mapGenerated.Invoke();
    }

    void GenerateHexagonalGrid()
    {
        for (int row = -_mapRadius; row <= _mapRadius; row++)
        {
            // Offset every other row by 0.5 in the X direction
            float rowOffset = row % 2 == 0 ? 0 : _deltaX * 0.5f;

            // Calculate the number of hexagons in this row
            int numHexes = _mapRadius * 2 + 1 - Mathf.Abs(row);

            // Adjust the starting column to center the hexagons
            int startCol = -(numHexes / 2);

            for (int col = startCol; col <= startCol + numHexes - 1; col++)
            {
                float x = col * _deltaX + rowOffset;
                float z = row * _deltaZ; // Adjust for vertical spacing

                Vector3 position = new Vector3(x, 0, z);
                GameObject tile = Instantiate(_tilePrefab, position, Quaternion.identity, _grid);
                tile.GetComponent<Tile>().Coordinate = new Vector2(col, row);
                tile.name = tile.GetComponent<Tile>().TileData.TileName + " " + col + ";" + row;

                //Add tile to dictionnary
                _tiles[new Vector2(col, row)] = tile.GetComponent<Tile>();
            }
        }
    }
}

public enum Biome
{
    Grassland, DeepForest, Mountain, Desert, Swamp, Ice 
}

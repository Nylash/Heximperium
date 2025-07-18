using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Map Configuration")]
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _emptyMap;
    //Tmp until game configuration menu
    [SerializeField] private int _mapRadius;
    [SerializeField] private GameObject _predefinedMap;
    #endregion

    #region VARIABLES
    //Offset for grid generation
    private float _deltaX = 1f;
    private float _deltaZ = 0.91f;

    private Transform _grid;
    private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();
    #endregion

    #region EVENTS
    public event Action OnMapGenerated;
    #endregion

    #region ACCESSORS
    public Dictionary<Vector2, Tile> Tiles { get => _tiles;}
    #endregion

    void Start()
    {
        if(_predefinedMap != null)
        {
            _grid = Instantiate(_predefinedMap).transform;

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

        OnMapGenerated?.Invoke();
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
                Tile tile = Instantiate(_tilePrefab, position, Quaternion.identity, _grid).GetComponent<Tile>();
                tile.Coordinate = new Vector2(col, row);

                //LOGIC to do place tile types (Basic, Hazard...) by setting TileData (name & income) are set in the setter
                //Think to set tile.InitialData and remove it from Tile.Awake()
                tile.name = tile.TileData.TileName + " " + col + ";" + row;
                tile.Incomes = tile.TileData.Incomes;

                //Add tile to dictionnary
                _tiles[new Vector2(col, row)] = tile;
            }
        }
    }
}

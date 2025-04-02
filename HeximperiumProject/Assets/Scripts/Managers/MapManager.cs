using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    [SerializeField] private GameObject _tilePrefab; // The prefab to instantiate at each hexagon position
    [SerializeField] private int _mapRadius = 1; // The radius of the hexagonal grid
    private float _deltaX = 1f; // Delta in X direction
    private float _deltaZ = 0.91f; // Delta in Z direction
    private Transform _grid;

    void Start()
    {
        _grid = GameObject.FindGameObjectWithTag("Grid").transform;
        GenerateHexagonalGrid();
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
                tile.name = col + ";" + row;
            }
        }
    }
}

public enum Biome
{
    Grassland, DeepForest, Mountain, Desert, Swamp, Ice 
}

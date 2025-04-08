using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    [SerializeField] private Biome _biome;
    [SerializeField] private TileData _tileData;

    //Remove the serializedField when the map creation is fixed
    [SerializeField] private Vector2 _coordinate;
    [SerializeField] private List<ResourceValue> _incomes = new List<ResourceValue>();

    private Tile[] _neighbors = new Tile[6];
    private bool _revealed;
    private bool _claimed;
    private Border _border;
    private Animator _animator;
    private List<Scout> _scouts = new List<Scout>();
    
    private TextMeshPro _scoutCounter;

    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }
    public TileData TileData
    {
        get => _tileData;
        set
        {
            if (value.TypeIncomeUpgrade == TypeIncomeUpgrade.Merge)
                _incomes = Utilities.MergeResourceValues(_incomes, value.Incomes);
            else
                _incomes = value.Incomes;
            name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
            _tileData = value;
            UpdateVisual();
        }
    }
    public bool Claimed { get => _claimed;}
    public bool Revealed { get => _revealed;}
    public Biome Biome { get => _biome; set => _biome = value; }
    public Tile[] Neighbors { get => _neighbors;}
    public List<Scout> Scouts { get => _scouts; set => _scouts = value; }
    public List<ResourceValue> Incomes { get => _incomes; set => _incomes = value; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void ClaimTile()
    {
        _claimed = true;
        _border = Instantiate(Resources.Load<GameObject>("Border"), transform.position, Quaternion.identity).GetComponent<Border>();
        _border.transform.parent = ExpansionManager.Instance.BorderParent;
        foreach (Tile neighbor in _neighbors) 
        {
            if (!neighbor.Revealed)
                neighbor.RevealTile(false);
        }
    }

    public void RevealTile(bool skipAnim)
    {
        _revealed = true;
        if (skipAnim)
            _animator.SetTrigger("InstantReveal");
        else
            _animator.SetTrigger("Reveal");
    }

    public void CheckBorder()
    {
        _border.CheckBorderVisibility(_neighbors);
    }

    private void UpdateVisual()
    {
        if (_tileData.name == "Water")
        {
            //Material not linked to biome
            GetComponent<Renderer>().material = Resources.Load(_tileData.name) as Material;
            return;
        }
        GetComponent<Renderer>().material = Resources.Load("TilesMaterial/" + _biome + "/" + _tileData.name + "_" + _biome) as Material;
    }

    public void SearchNeighbors()
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

    public void UpdateScoutCounter()
    {
        if(_scouts.Count >= 2)
        {
            if(_scoutCounter == null)
                _scoutCounter = Instantiate(ExplorationManager.Instance.ScoutCounterPrefab, transform).GetComponent<TextMeshPro>();
            _scoutCounter.text = _scouts.Count.ToString();
        }
        else
        {
            if (_scoutCounter != null)
                Destroy(_scoutCounter.gameObject);
        }
    }
}

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    #region CONFIGURATION
    [SerializeField] private GameObject _borderPrefab;
    #endregion

    #region VARIABLES
    //Remove the serializedField when the map creation is fixed
    [SerializeField] private Biome _biome;
    [SerializeField] private TileData _tileData;
    [SerializeField] private Vector2 _coordinate;
    [SerializeField] private List<ResourceValue> _incomes = new List<ResourceValue>();

    private Tile[] _neighbors = new Tile[6];
    private TileData _initialData;
    private bool _revealed;
    private bool _claimed;
    private Border _border;
    private Animator _animator;
    private List<Scout> _scouts = new List<Scout>();
    private TextMeshPro _scoutCounter;
    private Entertainer _entertainer;
    #endregion

    #region EVENTS
    //previous Incomes, new Incomes
    [HideInInspector] public UnityEvent<List<ResourceValue>, List<ResourceValue>> OnIncomeModified = new UnityEvent<List<ResourceValue>, List<ResourceValue>>();
    #endregion

    #region ACCESSORS
    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }
    public TileData TileData
    {
        get => _tileData;
        set
        {
            //Remove previous special behaviour
            if (_tileData.SpecialBehaviour != null)
            {
                _tileData.SpecialBehaviour.RollbackSpecialBehaviour();
            }

            if (value.TypeIncomeUpgrade == TypeIncomeUpgrade.Merge)
                Incomes = Utilities.MergeResourceValues(_incomes, value.Incomes);
            else
                Incomes = value.Incomes;
            name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
            _tileData = value;
            UpdateVisual();

            //Tile special behaviour
            if (_tileData.SpecialBehaviour != null)
            {
                _tileData.SpecialBehaviour.Tile = this;
                _tileData.SpecialBehaviour.InitializeSpecialBehaviour();
            }
                
            //Foreach neighbors check if their special behaviour should impact the new tile
            foreach (Tile item in _neighbors)
            {
                if(item.TileData.SpecialBehaviour != null)
                    item.TileData.SpecialBehaviour.ApplySpecialBehaviour(this);
            }
        }
    }
    public bool Claimed { get => _claimed;}
    public bool Revealed { get => _revealed;}
    public Biome Biome { get => _biome; set => _biome = value; }
    public Tile[] Neighbors { get => _neighbors;}
    public List<Scout> Scouts { get => _scouts; set => _scouts = value; }
    public List<ResourceValue> Incomes
    {
        get => _incomes;
        set
        {
            OnIncomeModified.Invoke(_incomes, value);
            _incomes = value;
        }
    }
    public TileData InitialData { get => _initialData; set => _initialData = value; }
    public Entertainer Entertainer { get => _entertainer; set => _entertainer = value; }
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _initialData = _tileData;
    }

    //Claim the tile and spawn the territory boundaries
    public void ClaimTile()
    {
        _claimed = true;
        _border = Instantiate(_borderPrefab, transform.position, Quaternion.identity).GetComponent<Border>();
        _border.transform.parent = ExpansionManager.Instance.BorderParent;
        foreach (Tile neighbor in _neighbors) 
        {
            if (!neighbor.Revealed)
                neighbor.RevealTile(false);
        }
    }

    //Reveal the tile, with or without the flipping animation
    public void RevealTile(bool skipAnim)
    {
        _revealed = true;
        if (skipAnim)
            _animator.SetTrigger("InstantReveal");
        else
            _animator.SetTrigger("Reveal");
    }

    //Called when a tile is claimed
    public void CheckBorder()
    {
        _border.CheckBorderVisibility(_neighbors);
    }

    //Change tile's visual based on the tile data
    private void UpdateVisual()
    {
        List<Material> materials = _tileData.GetMaterials(_biome);

        switch (materials.Count)
        {
            case 0:
                Debug.LogError("This tile " + _tileData + " has no material configured for this biome " + _biome);
                break;
            case 1:
                GetComponent<Renderer>().material = materials[0];
                break;
            default:
                GetComponent<Renderer>().material = materials[UnityEngine.Random.Range(0, materials.Count)];
                break;
        }
    }

    #region NEIGHBORS LOGIC
    //Only called at the map generation
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
    #endregion

    //Method used manage the scout counter of the tile (if there is several scouts on the same tile)
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

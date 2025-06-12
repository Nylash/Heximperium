using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    #region CONFIGURATION
    [SerializeField] private GameObject _borderPrefab;
    [SerializeField] private GameObject _highlightBoostPrefab;
    #endregion

    #region VARIABLES
    //Remove the serializedField when the map creation is fixed
    [SerializeField] private TileData _tileData;
    [SerializeField] private Vector2 _coordinate;
    [SerializeField] private List<ResourceToIntMap> _incomes = new List<ResourceToIntMap>();

    private Tile[] _neighbors = new Tile[6];
    private TileData _initialData;
    private bool _revealed;
    private bool _claimed;
    private Border _border;
    private Animator _animator;
    private List<Scout> _scouts = new List<Scout>();
    private TextMeshPro _scoutCounter;
    private Entertainer _entertainer;
    private GameObject _highlightBoostObject;
    #endregion

    #region EVENTS
    //previous Incomes, new Incomes
    [HideInInspector] public UnityEvent<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>> OnIncomeModified = new UnityEvent<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>>();
    [HideInInspector] public UnityEvent<Tile> OnTileClaimed = new UnityEvent<Tile>();
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
                _tileData.SpecialBehaviour.RollbackSpecialBehaviour(this);
            }

            //Set the new income
            if (value.TypeIncomeUpgrade == TypeIncomeUpgrade.Merge)
                Incomes = Utilities.MergeResourceValues(_incomes, value.Incomes);
            else
                Incomes = value.Incomes;

            name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
            _tileData = value;

            UpdateVisual();

            CheckSpecialBehaviour();
        }
    }
    public bool Claimed { get => _claimed;}
    public bool Revealed { get => _revealed;}
    public Tile[] Neighbors { get => _neighbors;}
    public List<Scout> Scouts { get => _scouts; set => _scouts = value; }
    public List<ResourceToIntMap> Incomes
    {
        get => _incomes;
        set
        {
            OnIncomeModified.Invoke(this, _incomes, value);
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
        OnTileClaimed.Invoke(this);
        _border = Instantiate(_borderPrefab, transform.position, Quaternion.identity).GetComponent<Border>();
        _border.transform.parent = ExpansionManager.Instance.BorderParent;
        foreach (Tile neighbor in _neighbors) 
        {
            if (!neighbor)
                continue;
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
        switch (_tileData.Visuals.Count)
        {
            case 0:
                Debug.LogError("This tile has no material configured");
                break;
            case 1:
                GetComponent<Renderer>().material = _tileData.Visuals[0];
                break;
            default:
                GetComponent<Renderer>().material = _tileData.Visuals[UnityEngine.Random.Range(0, _tileData.Visuals.Count)];
                break;
        }
    }

    public void BoostHighlight(bool show)
    {
        if (show)
            _highlightBoostObject = Instantiate(_highlightBoostPrefab, transform.position + new Vector3(0, 0.02f, 0), Quaternion.identity);
        else
            Destroy(_highlightBoostObject);
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


    #region SPECIAL BEHAVIOUR
    private void CheckSpecialBehaviour()
    {
        //Tile special behaviour
        if (_tileData.SpecialBehaviour != null)
        {
            _tileData.SpecialBehaviour.InitializeSpecialBehaviour(this);

            //Special case for IncomeComingFromNeighbors
            if (_tileData.SpecialBehaviour is IncomeComingFromNeighbors)
            {
                foreach (Tile neighbor in _neighbors)
                {
                    if (!neighbor)
                        continue;
                    neighbor.OnIncomeModified.RemoveListener(AdjustIncomeFromNeighbor);
                    neighbor.OnIncomeModified.AddListener(AdjustIncomeFromNeighbor);
                    if (!neighbor.Claimed)
                    {
                        neighbor.OnTileClaimed.RemoveListener(AddClaimedTileIncome);
                        neighbor.OnTileClaimed.AddListener(AddClaimedTileIncome);
                    } 
                }
            }
        }

        //Foreach neighbors check if their special behaviour should impact the new tile
        foreach (Tile item in _neighbors)
        {
            if (!item)
                continue;
            if (item.TileData.SpecialBehaviour != null)
                item.TileData.SpecialBehaviour.ApplySpecialBehaviour(this);
        }
    }

    //Region for the special behaviour IncomeComingFromNeighbors, 
    #region IncomeComingFromNeighbors
    private void AddClaimedTileIncome(Tile tile)
    {
        if (_tileData.SpecialBehaviour is IncomeComingFromNeighbors specialBehaviour)
        {
            specialBehaviour.AddClaimedTileIncome(this, tile);
        }
    }

    private void AdjustIncomeFromNeighbor(Tile neighbor, List<ResourceToIntMap> previousIncome, List<ResourceToIntMap> newIncome)
    {
        if(_tileData.SpecialBehaviour is IncomeComingFromNeighbors specialBehaviour)
        {
            specialBehaviour.AdjustIncomeFromNeighbor(neighbor, this, previousIncome, newIncome);
        }
    }
    #endregion
    #endregion
}

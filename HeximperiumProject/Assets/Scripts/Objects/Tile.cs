using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class Tile : MonoBehaviour
{
    #region CONFIGURATION
    [SerializeField] private GameObject _borderPrefab;
    [SerializeField] private GameObject _highlightPrefab;
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
    private GameObject _highlightObject;
    private TileData _previousData;
    #endregion

    #region EVENTS
    //previous Incomes, new Incomes
    [HideInInspector] public UnityEvent<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>> OnIncomeModified = new UnityEvent<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>>();
    [HideInInspector] public UnityEvent<Tile> OnTileClaimed = new UnityEvent<Tile>();
    #endregion

    #region ACCESSORS
    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }
    public TileData TileData { get => _tileData; set => UpdateTileData(value); }
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
    public TileData PreviousData { get => _previousData; }
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _initialData = _tileData;
    }

    #region BASIC METHODS
    //Update the tile data and call every other methods that impact
    private void UpdateTileData(TileData value)
    {
        RollbackSpecialBehaviours();

        //Set the new income
        if (value.TypeIncomeUpgrade == TypeIncomeUpgrade.Merge)
            Incomes = Utilities.MergeResourceToIntMaps(_incomes, value.Incomes);
        else
            Incomes = value.Incomes;

        name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
        _previousData = _tileData;
        _tileData = value;

        UpdateVisual();

        UpdateSpecialBehaviours();
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

    //Claim the tile and spawn the territory boundaries
    public void ClaimTile()
    {
        _claimed = true;
        OnTileClaimed.Invoke(this);
        _border = Instantiate(_borderPrefab, transform.position, Quaternion.identity).GetComponent<Border>();
        _border.transform.parent = ExpansionManager.Instance.BorderParent;
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

    public void Highlight(bool show)
    {
        if (show)
        {
            if (_highlightObject != null)
                return;
            _highlightObject = Instantiate(_highlightPrefab, transform.position + new Vector3(0, 0.02f, 0), Quaternion.identity);
        }
        else if(_highlightObject != null)
            Destroy(_highlightObject);
    }

    //Method used manage the scout counter of the tile (if there is several scouts on the same tile)
    public void UpdateScoutCounter()
    {
        if (_scouts.Count >= 2)
        {
            if (_scoutCounter == null)
                _scoutCounter = Instantiate(ExplorationManager.Instance.ScoutCounterPrefab, transform).GetComponent<TextMeshPro>();
            _scoutCounter.text = _scouts.Count.ToString();
        }
        else
        {
            if (_scoutCounter != null)
                Destroy(_scoutCounter.gameObject);
        }
    }
    #endregion

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

    #region SPECIAL BEHAVIOUR
    private void UpdateSpecialBehaviours()
    {
        //Tile special behaviour
        if (_tileData.SpecialBehaviours.Count != 0)
        {
            foreach (SpecialBehaviour item in _tileData.SpecialBehaviours)
            {
                item.InitializeSpecialBehaviour(this);
            }
        }

        //Foreach neighbors check if their special behaviour should impact the new tile
        foreach (Tile item in _neighbors)
        {
            if (!item)
                continue;
            if (item.TileData.SpecialBehaviours.Count != 0)
            {
                foreach (SpecialBehaviour specialBev in item.TileData.SpecialBehaviours)
                {
                    specialBev.InitializeSpecialBehaviourToSpecificTile(this, item);
                }
            }
        }
    }

    private void RollbackSpecialBehaviours()
    {
        //Remove previous special behaviour
        if (_tileData.SpecialBehaviours.Count != 0)
        {
            foreach (SpecialBehaviour item in _tileData.SpecialBehaviours)
            {
                item.RollbackSpecialBehaviour(this);
            }
        }
        //Foreach neighbors check if their special behaviour should be rollbacked for this tile
        foreach (Tile item in _neighbors)
        {
            if (!item)
                continue;
            if (item.TileData.SpecialBehaviours.Count != 0)
            {
                foreach (SpecialBehaviour specialBev in item.TileData.SpecialBehaviours)
                {
                    specialBev.RollbackSpecialBehaviourToSpecificTile(this, item);
                }
            }
        }
    }

    #region MethodsForSpecificBehaviours
    public void AddClaimedTileIncome(Tile tile)
    {
        foreach (IncomeComingFromNeighbors behaviour in _tileData.SpecialBehaviours.OfType<IncomeComingFromNeighbors>())
        {
            behaviour.AddClaimedTileIncome(this, tile);
        }
    }

    public void AdjustIncomeFromNeighbor(Tile neighbor, List<ResourceToIntMap> previousIncome, List<ResourceToIntMap> newIncome)
    {
        foreach (IncomeComingFromNeighbors behaviour in _tileData.SpecialBehaviours.OfType<IncomeComingFromNeighbors>())
        {
            behaviour.AdjustIncomeFromNeighbor(this, neighbor, previousIncome, newIncome);
        }
    }

    public void CheckNewInfra(Tile tile)
    {
        foreach (BoostByInfraOccurrenceInEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostByInfraOccurrenceInEmpire>())
        {
            behaviour.CheckNewInfra(this, tile);
        }
    }

    public void CheckDestroyedInfra(Tile tile)
    {
        foreach (BoostByInfraOccurrenceInEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostByInfraOccurrenceInEmpire>())
        {
            behaviour.CheckDestroyedInfra(this, tile);
        }
    }
    #endregion
    #endregion
}

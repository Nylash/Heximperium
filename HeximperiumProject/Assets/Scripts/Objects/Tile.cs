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
    private Entertainment _entertainment;
    private EntertainmentData _previousEntertainmentData;
    private GameObject _highlightObject;
    private TileData _previousData;
    private int _uniqueInfraNeighborsCount;
    #endregion

    #region EVENTS
    //previous Incomes, new Incomes
    [HideInInspector] public UnityEvent<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>> OnIncomeModified = new UnityEvent<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>>();
    [HideInInspector] public UnityEvent<Tile> OnTileClaimed = new UnityEvent<Tile>();
    [HideInInspector] public UnityEvent<Tile> OnTileDataModified = new UnityEvent<Tile>();
    [HideInInspector] public UnityEvent<Tile> OnEntertainmentModified = new UnityEvent<Tile>();
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
    public Entertainment Entertainment 
    { 
        get => _entertainment; 
        set 
        {
            if (_entertainment != null)
                _previousEntertainmentData = _entertainment.Data;
            _entertainment = value;
            OnEntertainmentModified.Invoke(this);
        }  
    }
    public TileData PreviousData { get => _previousData; }
    public int UniqueInfraNeighborsCount { get => _uniqueInfraNeighborsCount; set => _uniqueInfraNeighborsCount = value; }
    public EntertainmentData PreviousEntertainmentData { get => _previousEntertainmentData; }
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
        if (value is InfrastructureData)
            Incomes = Utilities.MergeResourceToIntMaps(_incomes, value.Incomes);
        else
        {
            //We are going back to the initial data (basic tile, resource tile or hazardous tile) so we reset the income
            Incomes = Utilities.SubtractResourceToIntMaps(_incomes, _tileData.Incomes);
            //If the preivous data is an infra we were on an enhanced infra so we need to remove the base infra income too
            if(_previousData is InfrastructureData)
                Incomes = Utilities.SubtractResourceToIntMaps(_incomes, _previousData.Incomes);
        }

        name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
        _previousData = _tileData;
        _tileData = value;

        UpdateVisual();

        UpdateSpecialBehaviours();

        OnTileDataModified.Invoke(this);
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
    }

    #region SPECIFIC LISTENERS FOR BEHAVIOURS
    #region ON TILE CLAIMED
    public void ListenerOnTileClaimed_IncomeComingFromNeighbors(Tile tile)
    {
        foreach (IncomeComingFromNeighbors behaviour in _tileData.SpecialBehaviours.OfType<IncomeComingFromNeighbors>())
        {
            behaviour.CheckClaimedTile(this, tile);
        }
    }

    public void ListenerOnTileClaimed_BoostClaimedNeighborsIncome(Tile tile)
    {
        foreach (BoostClaimedNeighborsIncome behaviour in _tileData.SpecialBehaviours.OfType<BoostClaimedNeighborsIncome>())
        {
            behaviour.ApplyBoostToClaimedTile(this, tile);
        }
    }

    public void ListenerOnTileClaimed_IncomeWhenTileClaimed(Tile tile)
    {
        foreach (IncomeWhenTileClaimed behaviour in _tileData.SpecialBehaviours.OfType<IncomeWhenTileClaimed>())
        {
            behaviour.TileClaimed(this);
        }
    }
    #endregion

    #region ON TILE DATA MODIFIED
    public void ListenerOnTileDataModified_NeighborsBoostingIncome(Tile tile)
    {
        foreach (NeighborsBoostingIncome behaviour in _tileData.SpecialBehaviours.OfType<NeighborsBoostingIncome>())
        {
            behaviour.CheckNewData(this, tile);
        }
    }

    public void ListenerOnTileDataModified_BoostNeighborsIncome(Tile tile)
    {
        foreach (BoostNeighborsIncome behaviour in _tileData.SpecialBehaviours.OfType<BoostNeighborsIncome>())
        {
            behaviour.CheckNewData(tile);
        }
    }

    public void ListenerOnTileDataModified_BoostByUniqueInfraNeighbors(Tile tile)
    {
        foreach (BoostByUniqueInfraNeighbors behaviour in _tileData.SpecialBehaviours.OfType<BoostByUniqueInfraNeighbors>())
        {
            behaviour.CheckNewData(this);
        }
    }

    public void ListenerOnTileDataModified_BoostNeighborsWithInfraIncome(Tile tile)
    {
        foreach (BoostNeighborsWithInfraIncome behaviour in _tileData.SpecialBehaviours.OfType<BoostNeighborsWithInfraIncome>())
        {
            behaviour.CheckNewData(tile);
        }
    }
    #endregion

    #region ON SCOUT SPAWNED
    public void ListenerOnScoutSpawned_BoostScoutOnSpawn(Scout scout)
    {
        foreach (BoostScoutOnSpawn behaviour in _tileData.SpecialBehaviours.OfType<BoostScoutOnSpawn>())
        {
            behaviour.CheckScoutSpawned(this, scout);
        }
    }

    public void ListenerOnScoutSpawned_GainIncomeWhenScoutRevealTile(Scout scout)
    {
        foreach (IncomeWhenScoutRevealTile behaviour in _tileData.SpecialBehaviours.OfType<IncomeWhenScoutRevealTile>())
        {
            behaviour.CheckScoutSpawned(this, scout);
        }
    }
    #endregion

    #region ON INFRA BUILDED
    public void ListenerOnInfraBuilded_BoostByInfraOccurrenceInEmpire(Tile tile)
    {
        foreach (BoostByInfraOccurrenceInEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostByInfraOccurrenceInEmpire>())
        {
            behaviour.CheckNewInfra(this, tile);
        }
    }

    public void ListenerOnInfraBuilded_BoostInfraOnEmpire(Tile tile)
    {
        foreach (BoostInfraOnEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostInfraOnEmpire>())
        {
            behaviour.CheckNewInfra(this, tile);
        }
    }
    #endregion

    #region ON INFRA DESTROYED
    public void ListenerOnInfraDestroyed_BoostByInfraOccurrenceInEmpire(Tile tile)
    {
        foreach (BoostByInfraOccurrenceInEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostByInfraOccurrenceInEmpire>())
        {
            behaviour.CheckDestroyedInfra(this, tile);
        }
    }

    public void ListenerOnInfraDestroyed_BoostInfraOnEmpire(Tile tile)
    {
        foreach (BoostInfraOnEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostInfraOnEmpire>())
        {
            behaviour.CheckDestroyedInfra(this, tile);
        }
    }
    #endregion

    public void ListenerOnScoutRevealingTile(Tile tile)
    {
        foreach (IncomeWhenScoutRevealTile behaviour in _tileData.SpecialBehaviours.OfType<IncomeWhenScoutRevealTile>())
        {
            behaviour.TileRevealed(this);
        }
    }

    public void ListenerOnIncomeModified(Tile tile, List<ResourceToIntMap> previousIncome, List<ResourceToIntMap> newIncome)
    {
        foreach (IncomeComingFromNeighbors behaviour in _tileData.SpecialBehaviours.OfType<IncomeComingFromNeighbors>())
        {
            behaviour.CheckNewIncome(this, tile, previousIncome, newIncome);
        }
    }

    public void ListenerOnClaimSaved(int quantity)
    {
        foreach (IncomePerSavedClaim behaviour in _tileData.SpecialBehaviours.OfType<IncomePerSavedClaim>())
        {
            behaviour.IncomeForSavedClaim(this, quantity);
        }
    }
    #endregion
    #endregion
}

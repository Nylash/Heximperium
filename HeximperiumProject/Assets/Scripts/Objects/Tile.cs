using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Configuration")]
    [SerializeField] private GameObject _borderPrefab;
    [SerializeField] private GameObject _highlightPrefab;
    [SerializeField] private Transform _visual;
    [SerializeField] private SpriteRenderer _infraLvlRenderer;
    [SerializeField] private Sprite[] _spriteInfraLvl = new Sprite[3];
    [SerializeField] private Animator _claimTintAnimator;
    [SerializeField] private TextMeshPro[] _incomesUI = new TextMeshPro[6];
    #endregion

    #region VARIABLES
    //Remove the serializedField when the map creation is fixed
    [Header("_________________________________________________________")]
    [Header("Map Generation Only")]
    [SerializeField] private TileData _tileData;
    [SerializeField] private Vector2 _coordinate;
    [SerializeField] private List<ResourceToIntMap> _incomes = new List<ResourceToIntMap>();

    private int _claimIncome = 0;
    private Tile[] _neighbors = new Tile[6];
    private TileData _initialData;
    private TileData _previousData;
    private bool _revealed;
    private bool _claimed;
    private Border _border;
    private Animator _animator;
    private GameObject _highlightObject;
    private int _currentInfraLevel = 0;
    private Coroutine _interactionCoroutine;
    private TileInteractionAnimationState _interactionAnimationState = TileInteractionAnimationState.None;
    //Scouts
    private List<Scout> _scouts = new List<Scout>();
    //Entertainment variables
    private bool _allowEntertainment;
    private Entertainment _entertainment;
    private Entertainment _previousEntertainment;//Only stay one frame (because the ref is deleted) but needed to clean the group (BoostByZone special effect)
    private EntertainmentData _previousEntertainmentData;

    private int _uniqueInfraNeighborsCount;
    private int _uniqueEntertainmentNeighborsCount_SB;//Count for special behaviour script
    private int _uniqueEntertainmentNeighborsCount_SE;//Count for special effect script, two count is needed if an entertainment and infra on the same tile use it
    private int _groupID;//Use for BoostByZoneSize entertainment's special effect
    #endregion

    #region EVENTS
    //previous Incomes, new Incomes
    public event Action<Tile, List<ResourceToIntMap>, List<ResourceToIntMap>> OnIncomeModified;
    public event Action<Tile> OnTileClaimed;
    public event Action<Tile> OnTileDataModified;
    public event Action<Tile> OnEntertainmentModified;
    public Action OnClaimBorderAnimationDone;//No event keyword because it is Invoked in the Border script
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
            OnIncomeModified?.Invoke(this, _incomes, value);
            _incomes = value;
            if (UIManager.Instance.AreIncomesShown)
                ShowIncomeUI(true);
        }
    }
    public TileData InitialData { get => _initialData; set => _initialData = value; }
    public Entertainment Entertainment 
    { 
        get => _entertainment; 
        set 
        {
            if (_entertainment != null)
            {
                _previousEntertainmentData = _entertainment.Data;
                _previousEntertainment = _entertainment;
            }
            else
                _previousEntertainmentData = null;
            _entertainment = value;
            OnEntertainmentModified?.Invoke(this);

            if (UIManager.Instance.AreIncomesShown)
                ShowIncomeUI(true);
        }  
    }
    public TileData PreviousData { get => _previousData; }
    public int UniqueInfraNeighborsCount { get => _uniqueInfraNeighborsCount; set => _uniqueInfraNeighborsCount = value; }
    public EntertainmentData PreviousEntertainmentData { get => _previousEntertainmentData; }
    public int UniqueEntertainmentNeighborsCount_SB { get => _uniqueEntertainmentNeighborsCount_SB; set => _uniqueEntertainmentNeighborsCount_SB = value; }
    public int UniqueEntertainmentNeighborsCount_SE { get => _uniqueEntertainmentNeighborsCount_SE; set => _uniqueEntertainmentNeighborsCount_SE = value; }
    public int GroupID { get => _groupID; set => _groupID = value; }
    public Entertainment PreviousEntertainment { get => _previousEntertainment; }
    public Animator Animator { get => _animator; }
    public Transform Visual { get => _visual; }
    public Coroutine InteractionCoroutine { get => _interactionCoroutine; set => _interactionCoroutine = value; }
    public TileInteractionAnimationState InteractionAnimationState { get => _interactionAnimationState; set => _interactionAnimationState = value; }
    public int ClaimIncome
    {
        get => _claimIncome;
        set
        {
            _claimIncome = value;
            if (UIManager.Instance.AreIncomesShown)
                ShowIncomeUI(true);
        }
    }

    public bool AllowEntertainment { get => _allowEntertainment; set => _allowEntertainment = value; }
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _initialData = _tileData;

        MapManager.Instance.OnMapGenerated += () =>
        {
            foreach (Tile neighbor in _neighbors)
            {
                if (!neighbor)
                    continue;
                neighbor.OnClaimBorderAnimationDone += CheckBorder;
            }
        };
    }

    #region BASIC METHODS
    public void InitializeTile(TileData data)
    {
        _initialData = data;
        _tileData = data;
        name = _tileData.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
        _incomes = data.Incomes;
        UpdateVisual();
    }

    //Update the tile data and call every other methods that impact
    private void UpdateTileData(TileData value)
    {
        RollbackSpecialBehaviours();

        //Set the new income
        if (value is InfrastructureData)
        {
            Incomes = Utilities.MergeResourceToIntMaps(_incomes, value.Incomes);
            _currentInfraLevel++;
        }
        else
        {
            //We are going back to the initial data (basic tile, resource tile or hazardous tile) so we reset the income
            Incomes = Utilities.SubtractResourceToIntMaps(_incomes, _tileData.Incomes);
            //If the preivous data is an infra we were on an enhanced infra so we need to remove the base infra income too
            if(_previousData is InfrastructureData)
                Incomes = Utilities.SubtractResourceToIntMaps(_incomes, _previousData.Incomes);
            _currentInfraLevel = 0;
        }

        name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
        _previousData = _tileData;
        _tileData = value;

        UpdateVisual();

        UpdateSpecialBehaviours();

        if (UIManager.Instance.AreIncomesShown)
            ShowIncomeUI(true);
        OnTileDataModified?.Invoke(this);
    }

    //Reveal the tile, with or without the flipping animation
    public void RevealTile(bool skipAnim)
    {
        _revealed = true;
        if (skipAnim)
            _animator.SetTrigger("InstantReveal");
        else
            _animator.SetTrigger("Reveal");
        ExplorationManager.Instance.RevealedTiles.Add(this);
    }

    //Claim the tile and spawn the territory boundaries
    public void ClaimTile()
    {
        if (!_revealed)
            RevealTile(false);

        _claimed = true;
        OnTileClaimed?.Invoke(this);
        _border = Instantiate(_borderPrefab, _visual).GetComponent<Border>();
        _border.transform.localPosition += new Vector3(0, 0.01f, 0);
        _border.GetComponent<Border>().associatedTile = this;
        _claimTintAnimator.SetTrigger("Claim");
        _border.name = "Border" + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";

        if (UIManager.Instance.AreIncomesShown)
            ShowIncomeUI(true);
    }

    //Called when a tile is claimed
    public void CheckBorder()
    {
        if(_border)
            _border.CheckBorderVisibility();
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
                GetComponentInChildren<Renderer>().material = _tileData.Visuals[0];
                break;
            default:
                GetComponentInChildren<Renderer>().material = _tileData.Visuals[UnityEngine.Random.Range(0, _tileData.Visuals.Count)];
                break;
        }
        switch (_currentInfraLevel)
        {
            case 0:
                _infraLvlRenderer.sprite = null;
                break;
            case 1:
                _infraLvlRenderer.sprite = _spriteInfraLvl[0];
                break;
            case 2:
                _infraLvlRenderer.sprite = _spriteInfraLvl[1];
                break;
            case 3:
                _infraLvlRenderer.sprite = _spriteInfraLvl[2];
                break;
            default:
                break;
        }
    }

    public void Highlight(bool show)
    {
        if (show)
        {
            if (_highlightObject != null)
                return;
            _highlightObject = Instantiate(_highlightPrefab, _visual);
            _highlightObject.transform.localPosition += new Vector3(0, 0.05f, 0);
        }
        else if(_highlightObject != null)
        {
            _highlightObject.GetComponent<Animator>().SetTrigger("Destroy");
        }
    }

    public bool TileEnhanceable()
    {
        if (!_revealed)
            return false;
        if (!_claimed)
            return false;
        if (_tileData.AvailableInfrastructures.Count == 0)
            return false;
        foreach (InfrastructureData data in _tileData.AvailableInfrastructures)
        {
            if(ResourcesManager.Instance.CanAfford(data.Costs))
                return true;
        }
        return false;
    }

    public void ShowIncomeUI(bool show)
    {
        // Hide them all, to avoid leftovers
        foreach (TextMeshPro income in _incomesUI)
            income.transform.parent.gameObject.SetActive(false);

        if (!show) return;

        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
        {
            if (_entertainment != null)
            {
                _incomesUI[0].text = _entertainment.Points + "<sprite name=\"Point_Emoji\">";
                _incomesUI[0].transform.parent.gameObject.SetActive(true);
            }
            return;
        }

        int count = 0;

        if (_tileData.SpecialBehaviours.Any(b => b is BoostScoutsLimit))
        {
            _incomesUI[count].text = "1<sprite name=\"Scout_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }

        if (_claimIncome > 0)
        {
            _incomesUI[count].text = "+" + _claimIncome + "<sprite name=\"Claim_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }

        if (_tileData.SpecialBehaviours.Any(b => b is BoostTownsLimit))
        {
            _incomesUI[count].text = "1<sprite name=\"Town_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }

        var goldIncome = _incomes.GetValueFor(Resource.Gold);
        if (goldIncome is int value)
        {
            _incomesUI[count].text = "+" + value + "<sprite name=\"Gold_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }

        var srIncome = _incomes.GetValueFor(Resource.SpecialResources);
        if (srIncome is int val)
        {
            _incomesUI[count].text = "+" + val + "<sprite name=\"SR_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }

        var generateCarnivalist = _tileData.SpecialBehaviours.OfType<GenerateCarnivalist>().FirstOrDefault();
        if (generateCarnivalist != null)
        {
            _incomesUI[count].text = generateCarnivalist.CarnivalistQuantity + "<sprite name=\"Carnivalist_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }
    }

    public bool CanReceiveEntertainment()
    {
        if (!_claimed)
            return false;
        if (_allowEntertainment)
            return true;
        if (EntertainmentManager.Instance.UpgradeMinstrelStageOnNeighbor)
        {
            foreach (Tile neighbor in _neighbors)
            {
                if (!neighbor)
                    continue;
                if (neighbor.Entertainment != null)
                    return true;
            }
        }
        return false;
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

    public bool IsOneNeighborOfNeighborClaimed()
    {
        foreach (Tile neighbor in _neighbors)
        {
            if (!neighbor)
                continue;
            if(neighbor.IsOneNeighborClaimed())
                return true;
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

    // Call the specific listeners for each special behaviour, this is used to create a pair between the tile and the event inkover
    #region SPECIFIC LISTENERS FOR BEHAVIOURS
    #region ON ENTERTAINMENT MODIFIED
    public void ListenerOnEntertainmentModified_BoostNeighborEntertainments(Tile tile)
    {
        foreach (BoostNeighborEntertainments behaviour in _tileData.SpecialBehaviours.OfType<BoostNeighborEntertainments>())
        {
            behaviour.CheckNewEntertainment(tile);
        }
    }

    public void ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors(Tile tile)
    {
        foreach (BoostEntertainmentByUniqueNeighbors behaviour in _tileData.SpecialBehaviours.OfType<BoostEntertainmentByUniqueNeighbors>())
        {
            behaviour.CheckNewEntertainment(this);
        }
    }
    #endregion

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

    public void ListenerOnEntertainmentSpawned(Entertainment ent)
    {
        foreach (BoostEntertainmentsOnEmpire behaviour in _tileData.SpecialBehaviours.OfType<BoostEntertainmentsOnEmpire>())
        {
            behaviour.CheckEntertainment(ent);
        }
    }
    #endregion
    #endregion
}

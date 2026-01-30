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
    [SerializeField] private GameObject _previewBorderPrefab;
    [SerializeField] private GameObject _allowingEntPrefab;
    [SerializeField] private GameObject _enhanceableTilePrefab;
    [SerializeField] private GameObject _highlightPrefab;
    [SerializeField] private Transform _visual;
    [SerializeField] private SpriteRenderer _infraLvlRenderer;
    [SerializeField] private GameObject _maxInfraReached;
    [SerializeField] private Sprite[] _spriteInfraLvl = new Sprite[3];
    [SerializeField] private Animator _claimTintAnimator;
    [SerializeField] private TextMeshPro[] _incomesUI = new TextMeshPro[6];
    #endregion

    #region VARIABLES
    //Remove the serializedField when the map creation is implemented
    [Header("_________________________________________________________")]
    [Header("Map Generation Only")]
    [SerializeField] private TileData _tileData;
    [SerializeField] private Vector2 _coordinate;
    [SerializeField] private List<ResourceToIntMap> _incomes = new List<ResourceToIntMap>();

    //Objects
    private Tile[] _neighbors = new Tile[6];
    private Border _border;
    private Border _previewBorder;
    private Animator _animator;
    private GameObject _highlightObject;
    private GameObject _visualAssets;
    private Border _allowEntHint;
    private Border _enhanceableHint;
    //Runtime variables
    private TileData _initialData;
    private TileData _previousData;
    private bool _revealed;
    private bool _claimed;
    private int _claimIncome = 0;
    private int _carnivalistCostReduction;
    private int _recruitedCarnivalists;
    private int _bufferRecruitedCarnivalists;
    private int _infraFromTurn = -1;
    private List<ResourceToIntMap> _incomeWithPreviousData = new List<ResourceToIntMap>();
    private Dictionary<BoostByUniqueInfraNeighbors, HashSet<Tile>> _uniqueInfraNeighborsByBehaviour = new Dictionary<BoostByUniqueInfraNeighbors, HashSet<Tile>>();
    //Variables for boucing animations (no more used)
    private Coroutine _interactionCoroutine;
    private TileInteractionAnimationState _interactionAnimationState = TileInteractionAnimationState.None;
    //Dictionary for impacted tiles and sources
    private Dictionary<Tile, List<ResourceToIntMap>> _externalIncomesSources = new Dictionary<Tile, List<ResourceToIntMap>>(); // Income coming from other tiles behaviours
    private Dictionary<Tile, List<ResourceToIntMap>> _internalIncomesSources = new Dictionary<Tile, List<ResourceToIntMap>>(); // Income coming from this tile behaviours
    private Dictionary<Tile, int> _internalCarnivalistsSources = new Dictionary<Tile, int>(); // There is only internal source of carnivalists
    private Dictionary<Tile, List<ResourceToIntMap>> _impactedTilesIncomes = new Dictionary<Tile, List<ResourceToIntMap>>();
    private Dictionary<Tile, int> _impactedTilesCarnivalists = new Dictionary<Tile, int>();
    private Dictionary<Tile, int> _entImpactedByEntertainment = new Dictionary<Tile, int>();//Use the tile to avoid issues with destroyed entertainment
    private Dictionary<Tile, int> _entImpactedByTile = new Dictionary<Tile, int>();//Use the tile to avoid issues with destroyed entertainment
    //Scouts
    private List<Scout> _scouts = new List<Scout>();
    //Entertainment variables
    private bool _allowEntertainment;
    private Entertainment _entertainment;
    private Entertainment _previousEntertainment;//Only stay one frame (because the ref is deleted) but needed to clean the group (BoostByZone special effect)
    private EntertainmentData _previousEntertainmentData;
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
    public Action OnPreviewBorderAnimationDone;//No event keyword because it is Invoked in the Border script
    public Action OnAllowingEntAnimationDone;//No event keyword because it is Invoked in the Border script
    public Action OnEnhanceableTileAnimationDone;//No event keyword because it is Invoked in the Border script
    #endregion

    #region ACCESSORS
    public Vector2 Coordinate { get => _coordinate; set => _coordinate = value; }
    public TileData TileData { get => _tileData; }
    public bool Claimed { get => _claimed;}
    public bool Revealed { get => _revealed;}
    public Tile[] Neighbors { get => _neighbors;}
    public List<Scout> Scouts { get => _scouts; set => _scouts = value; }
    public List<ResourceToIntMap> Incomes { get => _incomes; }
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

            UpdateShowAllowEntHint();
        }  
    }
    public TileData PreviousData { get => _previousData; set => _previousData = value; }
    public EntertainmentData PreviousEntertainmentData { get => _previousEntertainmentData; }
    public int UniqueEntertainmentNeighborsCount_SE { get => _uniqueEntertainmentNeighborsCount_SE; set => _uniqueEntertainmentNeighborsCount_SE = value; }
    public int GroupID { get => _groupID; set => _groupID = value; }
    public Entertainment PreviousEntertainment { get => _previousEntertainment; }
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
    public bool AllowEntertainment 
    { 
        get => _allowEntertainment;
        set
        {
            _allowEntertainment = value;
            if (!_allowEntertainment)
            {
                if (EntertainmentManager.Instance.UpgradeAllowEntOnSpecificInfra != null)
                    EntertainmentManager.Instance.UpgradeAllowEntOnSpecificInfra.CheckData(this);
            }
            UpdateShowAllowEntHint();
        }
    }
    public int CarnivalistCostReduction { get => _carnivalistCostReduction; set => _carnivalistCostReduction = value; }
    public int RecruitedCarnivalists { get => _recruitedCarnivalists; }
    public Dictionary<Tile, List<ResourceToIntMap>> ExternalIncomesSources { get => _externalIncomesSources; }
    public Dictionary<Tile, int> EntImpactedByEntertainment { get => _entImpactedByEntertainment; }
    public Dictionary<Tile, int> EntImpactedByTile { get => _entImpactedByTile; }
    public Dictionary<BoostByUniqueInfraNeighbors, HashSet<Tile>> UniqueInfraNeighborsByBehaviour { get => _uniqueInfraNeighborsByBehaviour; }
    public Dictionary<Tile, List<ResourceToIntMap>> ImpactedTilesIncomes { get => _impactedTilesIncomes; }
    public Dictionary<Tile, int> ImpactedTilesCarnivalists { get => _impactedTilesCarnivalists; }
    public Dictionary<Tile, List<ResourceToIntMap>> InternalIncomesSources { get => _internalIncomesSources; }
    public List<ResourceToIntMap> IncomeWithPreviousData { get => _incomeWithPreviousData; }
    public Dictionary<Tile, int> InternalCarnivalistsSources { get => _internalCarnivalistsSources; }
    public int InfraFromTurn { get => _infraFromTurn; set => _infraFromTurn = value; }
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
                neighbor.OnPreviewBorderAnimationDone += CheckPreviewBorder;
                neighbor.OnAllowingEntAnimationDone += CheckAllowingEnt;
                neighbor.OnEnhanceableTileAnimationDone += CheckEnhanceableStatus;
                neighbor.OnEntertainmentModified += (tile) => UpdateShowAllowEntHint();
            }
        };
    }

    private void LateUpdate()
    {
        if (_bufferRecruitedCarnivalists != _recruitedCarnivalists)
        {
            if (_recruitedCarnivalists < _bufferRecruitedCarnivalists)
                ResourcesManager.Instance.HelperOnCarnivalistSpent(this, _bufferRecruitedCarnivalists - _recruitedCarnivalists);
            else
                ResourcesManager.Instance.HelperOnCarnivalistGained(this, _recruitedCarnivalists - _bufferRecruitedCarnivalists);
        }
        _bufferRecruitedCarnivalists = _recruitedCarnivalists;
    }

    #region BASIC METHODS
    public void InitializeTile(TileData data)
    {
        _initialData = data;
        _tileData = data;
        name = _tileData.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
        _incomes = Utilities.CloneResourceToIntMaps(data.Incomes);
    }

    public void UpdateIncomes(List<ResourceToIntMap> inputIncomes, bool merge, Tile extSource = null, Tile intSource = null)
    {
        if (extSource != null && intSource != null)
        {
            Debug.LogError("A tile income update can't have both an external and internal source");
            return;
        }

        List<ResourceToIntMap> previousIncomes = Utilities.CloneResourceToIntMaps(_incomes);

        if (merge)
        {
            _incomes = Utilities.MergeResourceToIntMaps(_incomes, inputIncomes);
        }
        else
        {
            _incomes = Utilities.SubtractResourceToIntMaps(_incomes, inputIncomes);
        }

        if (extSource)
        {
            UpdateSourceOrImpactedTiles(_externalIncomesSources, merge, extSource, inputIncomes, true);
        }
        else if (intSource)
        {
            UpdateSourceOrImpactedTiles(_internalIncomesSources, merge, intSource, inputIncomes, true);
        }

        OnIncomeModified?.Invoke(this, Utilities.CloneResourceToIntMaps(previousIncomes), Utilities.CloneResourceToIntMaps(_incomes));
        if (UIManager.Instance.AreIncomesShown)
            ShowIncomeUI(true);
    }

    public void UpdateCarnivalists(int value, Tile source = null)
    {
        _recruitedCarnivalists += value;

        if (source != null)
        {
            UpdateSourceCarnivalists(source, value);
            source.UpdateImpactedTilesCarnivalists(this, value);
        }

        if (UIManager.Instance.AreIncomesShown)
            ShowIncomeUI(true);
    }

    public List<ResourceToIntMap> GetIncomeFromTileOnly()
    {
        if (_incomes.Count == 0)
            return new List<ResourceToIntMap>();
        if (_externalIncomesSources.Count == 0)
            return Utilities.CloneResourceToIntMaps(_incomes);

        List<ResourceToIntMap> incomeFromTileOnly = new List<ResourceToIntMap>();
        incomeFromTileOnly = Utilities.MergeResourceToIntMaps(incomeFromTileOnly, _incomes);
        foreach (var kvp in _externalIncomesSources)
        {
            incomeFromTileOnly = Utilities.SubtractResourceToIntMaps(incomeFromTileOnly, kvp.Value);
        }
        return incomeFromTileOnly;
    }

    //Update the tile data and call every other methods that impact
    public void UpdateTileData(TileData value, bool updateVisual)
    {
        _incomeWithPreviousData = Utilities.CloneResourceToIntMaps(_incomes);

        RollbackSpecialBehaviours();

        //Set the new income
        if (value is InfrastructureData)
        {
            UpdateIncomes(value.Incomes, true);
        }
        else
        {
            //We are going back to the initial data (basic tile, resource tile or hazardous tile) so we reset the income
            UpdateIncomes(_tileData.Incomes, false);
            //If the preivous data is an infra we were on an enhanced infra so we need to remove the base infra income too
            if(_previousData is InfrastructureData)
                UpdateIncomes(_previousData.Incomes, false);
        }

        name = value.TileName + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
        _previousData = _tileData;
        _tileData = value;

        if (updateVisual)
        {
            _animator.SetTrigger("UpdateVisual");
            if (UIManager.Instance.AreEnhanceableStatusShown)
                ShowEnhanceableTileStatus(true);
        }

        UpdateSpecialBehaviours();

        if (UIManager.Instance.AreIncomesShown)
            ShowIncomeUI(true);
        OnTileDataModified?.Invoke(this);
    }

    //Reveal the tile, with or without the flipping animation
    public void RevealTile(bool skipAnim)
    {
        _revealed = true;
        UpdateVisual();
        if (skipAnim)
            _animator.SetTrigger("InstantReveal");
        else
            _animator.SetTrigger("Reveal");
        ExplorationManager.Instance.RevealedTiles.Add(this);
        if (UIManager.Instance.AreEntPlacementShown)
            ShowEntPlacementUI(true);
        if (UIManager.Instance.AreIncomesShown)
            ShowIncomeUI(true);
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

        PreviewBorder(false, true);
    }

    public void PreviewBorder(bool activate, bool quickDeath = false)
    {
        if (activate)
        {
            if (_previewBorder != null)
            {
                CheckPreviewBorder();
                return;
            }
            _previewBorder = Instantiate(_previewBorderPrefab, _visual).GetComponent<Border>();
            _previewBorder.transform.localPosition += new Vector3(0, 0.01f, 0);
            _previewBorder.GetComponent<Border>().associatedTile = this;
            _previewBorder.name = "PreviewBorder" + " (" + (int)_coordinate.x + ";" + (int)_coordinate.y + ")";
            CheckPreviewBorder();
        }
        else if(_previewBorder != null)
        {
            if (!quickDeath)
                _previewBorder.GetComponent<Animator>().SetTrigger("Destroy");
            else
                Destroy(_previewBorder.gameObject);
            _previewBorder = null;
        }
    }

    //Called when a tile is claimed
    public void CheckBorder()
    {
        if (_border)
            _border.CheckBorderVisibility();
    }

    private void CheckPreviewBorder()
    {
        if (_previewBorder)
            _previewBorder.CheckPreviewBorderVisibility();
    }

    private void CheckAllowingEnt()
    {
        if (_allowEntHint)
            _allowEntHint.CheckAllowingEntVisibility();
    }

    private void CheckEnhanceableStatus()
    {
        if (_enhanceableHint)
            _enhanceableHint.CheckEnhanceableTileVisibility();
    }

    //Change tile's visual based on the tile data
    public void UpdateVisual()
    {
        if (_tileData is InfrastructureData infraData)
        {
            _infraLvlRenderer.sprite = _spriteInfraLvl[infraData.InfrastructureLevel - 1];
            if (_tileData.AvailableInfrastructures.Count == 0)
            {
                _maxInfraReached.SetActive(true);
                PopUpManager.Instance.ShowInfraLevelTutoPopUp();
            }
            else
                _maxInfraReached.SetActive(false);
        }   
        else
        {
            _infraLvlRenderer.sprite = null;
            _maxInfraReached.SetActive(false);
        }

        if (_tileData is HazardousTileData)
            _claimTintAnimator.gameObject.SetActive(false);

        if (_visualAssets != null)
        {
            GameObject previousVisualAssets = _visualAssets;
            Destroy(previousVisualAssets);
        }
        foreach (TileDataToGameObjectsMap item in _tileData.VisualsProps)
        {
            if (item.tileData == _initialData)
            {
                _visualAssets = Instantiate(item.gameObjects[UnityEngine.Random.Range(0, item.gameObjects.Count)], _visual);
                return;
            }
        }
        Debug.LogWarning("No visual asset found for this initial tile data");
    }

    public void Highlight(bool show)
    {
        if (_tileData is HazardousTileData)
            return;

        if (show)
        {
            if (_highlightObject != null)
                return;
            _highlightObject = Instantiate(_highlightPrefab, _visual);
            if (Revealed)
                _highlightObject.transform.localPosition += new Vector3(0, 0.05f, 0);
            else
                _highlightObject.transform.localPosition += new Vector3(0, -0.05f, 0);
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

    private void UpdateShowAllowEntHint()
    {
        if (ExploitationManager.Instance.IsPredictingIncome || EntertainmentManager.Instance.IsPredictingPoints)
            return;
        if (UIManager.Instance.AreEntPlacementShown)
            ShowEntPlacementUI(true);
    }

    private void RemoveAllowEntHint()
    {
        _allowEntHint.GetComponent<Animator>().SetTrigger("Despawn");
        _allowEntHint = null;
        if (UIManager.Instance.AreEntPlacementShown)
            OnAllowingEntAnimationDone?.Invoke();
    }

    public void ShowEntPlacementUI(bool show)
    {
        if (_tileData is HazardousTileData)
            return;
        if (!CanReceiveEntertainment(true))
        {
            if (_allowEntHint != null)
                RemoveAllowEntHint();
            return;
        }
        if (show)
        {
            if (GameManager.Instance.CurrentPhase == Phase.Entertain && !_claimed)
            {
                if (_allowEntHint)
                    RemoveAllowEntHint();
                return;
            }
            else
            {
                if (_allowEntHint)
                {
                    _allowEntHint.CheckAllowingEntVisibility();
                    return;
                }
                _allowEntHint = Instantiate(_allowingEntPrefab, _visual).GetComponent<Border>();
                _allowEntHint.transform.localPosition += new Vector3(0, 0.01f, 0);
                _allowEntHint.associatedTile = this;
            }
        }
        else if (_allowEntHint)
        {
            RemoveAllowEntHint();
        }
    }

    public void ShowEnhanceableTileStatus(bool show)
    {
        if (_tileData is HazardousTileData)
            return;
        if (!_claimed)
            return;
        if (show)
        {
            if (_tileData.AvailableInfrastructures.Count == 0)
            {
                if (_enhanceableHint != null)
                    RemoveEnhanceableHint();
                return;
            }
            else
            {
                if (_enhanceableHint)
                {
                    _enhanceableHint.CheckEnhanceableTileVisibility();
                    return;
                }
                _enhanceableHint = Instantiate(_enhanceableTilePrefab, _visual).GetComponent<Border>();
                _enhanceableHint.transform.localPosition += new Vector3(0, 0.01f, 0);
                _enhanceableHint.associatedTile = this;
            }
        }
        else if (_enhanceableHint != null)
            RemoveEnhanceableHint();
    }

    private void RemoveEnhanceableHint()
    {
        _enhanceableHint.GetComponent<Animator>().SetTrigger("Despawn");
        _enhanceableHint = null;
        if (UIManager.Instance.AreEnhanceableStatusShown)
            OnEnhanceableTileAnimationDone?.Invoke();
    }

    public void ShowIncomeUI(bool show)
    {
        if (_tileData is HazardousTileData)
            return;

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

        BoostScoutsLimit scoutBoost = _tileData.SpecialBehaviours.OfType<BoostScoutsLimit>().FirstOrDefault();
        if (scoutBoost != null)
        {
            _incomesUI[count].text = scoutBoost.ScoutsIncrease + "<sprite name=\"Scout_Emoji\">";
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

        if (_recruitedCarnivalists > 0)
        {
            _incomesUI[count].text = _recruitedCarnivalists + "<sprite name=\"Carnivalist_Emoji\">";
            _incomesUI[count].transform.parent.gameObject.SetActive(true);
            count++;
        }
    }

    public bool CanReceiveEntertainment(bool ignoreClaimStatus = false)
    {
        if (TileData is HazardousTileData)
            return false;
        if (ignoreClaimStatus)
        {
            if (!_revealed)
                return false;
        }
        else
        {
            if (!_claimed)
                return false;
        }
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

    public bool CanAffordCheapestEntertainment()
    {
        if (!_claimed)
            return false;
        if (EntertainmentManager.Instance.MinstrelData.GetActualCarnivalistCost(this) <= ResourcesManager.Instance.Carnivalist)
            return true;
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

    #region IMPACTED TILES MANAGEMENT
    public void UpdateSourceOrImpactedTiles(Dictionary<Tile, List<ResourceToIntMap>> dictionary, bool merge, Tile refTile, List<ResourceToIntMap> inputIncomes, bool updateImpactedTiles)
    {
        if (merge)
        {
            if (!dictionary.ContainsKey(refTile))
                dictionary.Add(refTile, Utilities.CloneResourceToIntMaps(inputIncomes));
            else
                dictionary[refTile] = Utilities.MergeResourceToIntMaps(dictionary[refTile], inputIncomes);
        }
        else
        {
            if (dictionary.ContainsKey(refTile))
            {
                dictionary[refTile] = Utilities.SubtractResourceToIntMaps(dictionary[refTile], inputIncomes);
            }
            else
            {
                Debug.LogWarning("Trying to remove income from a source that doesn't exist in the dictionary");
                return;
            }
        }
        if (updateImpactedTiles)
            refTile.UpdateSourceOrImpactedTiles(refTile.ImpactedTilesIncomes, merge, this, inputIncomes, false);

        dictionary[refTile].RemoveAll(r => r.value == 0);
        if (dictionary[refTile].Count == 0)
            dictionary.Remove(refTile);
    }

    private void UpdateSourceCarnivalists(Tile source, int carnivalistsQuantity)
    {
        if (!_internalCarnivalistsSources.ContainsKey(source))
            _internalCarnivalistsSources.Add(source, carnivalistsQuantity);
        else
            _internalCarnivalistsSources[source] += carnivalistsQuantity;
        if (_internalCarnivalistsSources[source] <= 0)
            _internalCarnivalistsSources.Remove(source);
    }

    public void UpdateImpactedTilesCarnivalists(Tile tile, int carnivalistPoints)
    {
        if (!_impactedTilesCarnivalists.ContainsKey(tile))
            _impactedTilesCarnivalists.Add(tile, carnivalistPoints);
        else
            _impactedTilesCarnivalists[tile] += carnivalistPoints;
        if (_impactedTilesCarnivalists[tile] <= 0)
            _impactedTilesCarnivalists.Remove(tile);
    }

    public void UpdateImpactedEntertainmentByEntertainment(Tile tile, int entertainmentPoints)
    {
        if (!_entImpactedByEntertainment.ContainsKey(tile))
            _entImpactedByEntertainment.Add(tile, entertainmentPoints);
        else
            _entImpactedByEntertainment[tile] += entertainmentPoints;
        if (_entImpactedByEntertainment[tile] <= 0)
            _entImpactedByEntertainment.Remove(tile);
    }

    public void UpdateImpactedEntByTile(Tile tile, int entertainmentPoints)
    {
        if (!_entImpactedByTile.ContainsKey(tile))
            _entImpactedByTile.Add(tile, entertainmentPoints);
        else
            _entImpactedByTile[tile] += entertainmentPoints;
        if (_entImpactedByTile[tile] <= 0)
            _entImpactedByTile.Remove(tile);
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
    public void ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors(Tile tile)
    {
        foreach (BoostEntertainmentOnTileAndOnNeighbors behaviour in _tileData.SpecialBehaviours.OfType<BoostEntertainmentOnTileAndOnNeighbors>())
        {
            behaviour.CheckNewEntertainment(tile, this);
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
            behaviour.CheckNewData(tile, this);
        }
    }

    public void ListenerOnTileDataModified_BoostByUniqueInfraNeighbors(Tile tile)
    {
        foreach (BoostByUniqueInfraNeighbors behaviour in _tileData.SpecialBehaviours.OfType<BoostByUniqueInfraNeighbors>())
        {
            behaviour.CheckNewData(this);
        }
    }

    public void ListenerOnTileDataModified_IncomeComingFromNeighbors(Tile tile)
    {
        foreach (IncomeComingFromNeighbors behaviour in _tileData.SpecialBehaviours.OfType<IncomeComingFromNeighbors>())
        {
            behaviour.CheckNewData(this, tile, tile.PreviousData, tile.TileData, tile.IncomeWithPreviousData, tile.Incomes);
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

    public void ListenerOnInfraBuilded_GenerateCarnivalistPerOccurrenceInEmpire(Tile tile)
    {
        foreach (GenerateCarnivalistPerOccurrenceInEmpire behaviour in _tileData.SpecialBehaviours.OfType<GenerateCarnivalistPerOccurrenceInEmpire>())
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

    public void ListenerOnInfraDestroyed_GenerateCarnivalistPerOccurrenceInEmpire(Tile tile)
    {
        foreach (GenerateCarnivalistPerOccurrenceInEmpire behaviour in _tileData.SpecialBehaviours.OfType<GenerateCarnivalistPerOccurrenceInEmpire>())
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
    #endregion
    #endregion
}

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class EntertainmentManager : PhaseManager<EntertainmentManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Entertainment Data")]
    [SerializeField] private List<EntertainmentData> _entertainmentsData = new List<EntertainmentData>();
    [Tooltip("Minstrel stage data to access it easily with the upgrade (keep it in the list too)")]
    [SerializeField] private EntertainmentData _minstrelData;
    [SerializeField] private GameObject _entertainmentPrefab;
    [SerializeField] private Transform _entertainmentsParent;
    [Header("_________________________________________________________")]
    [Header("Resources Conversion")]
    [SerializeField] private int _goldForOneCarnivalist = 500;
    [SerializeField] private int _SrForOneCarnivalist = 100;
    [SerializeField] private TileData _emptyDataForString;
    #endregion

    #region VARIABLES
    private List<Entertainment> _entertainments = new List<Entertainment>();
    private int _score;
    private Dictionary<int, List<Entertainment>> _groupBoost = new Dictionary<int, List<Entertainment>>(); //Use for BoostByZoneSize special effect, <GroupID, Entertainments>
    private bool _isPredictingPoints;
    private Coroutine _resetPredictBoolCoroutine;
    // Upgrade variables
    private bool _upgradeMinstrelStageOnNeighbor;
    private AllowEntertainmentOnSpecificInfra _upgradeAllowEntOnSpecificInfra;
    #endregion

    #region ACCESSORS
    public List<Entertainment> Entertainments { get => _entertainments; }
    public int Score { get => _score; }
    public Dictionary<int, List<Entertainment>> GroupBoost { get => _groupBoost; }
    public bool UpgradeMinstrelStageOnNeighbor { get => _upgradeMinstrelStageOnNeighbor; set => _upgradeMinstrelStageOnNeighbor = value; }
    public EntertainmentData MinstrelData { get => _minstrelData; }
    public AllowEntertainmentOnSpecificInfra UpgradeAllowEntOnSpecificInfra { get => _upgradeAllowEntOnSpecificInfra; set => _upgradeAllowEntOnSpecificInfra = value; }
    public bool IsPredictingPoints { get => _isPredictingPoints; }
    public List<EntertainmentData> EntertainmentsData { get => _entertainmentsData; }

    public int GetPointsFromMinstrelStage()
    {
        int points = 0;
        foreach (Entertainment entertainment in _entertainments)
        {
            if (entertainment.Data.Type == EntertainmentType.MinstrelStage)
            {
                points += entertainment.Points;
            }
        }
        return points;
    }
    public int GetPointsFromTastingPavilion()
    {
        int points = 0;
        foreach (Entertainment entertainment in _entertainments)
        {
            if (entertainment.Data.Type == EntertainmentType.TastingPavilion)
            {
                points += entertainment.Points;
            }
        }
        return points;
    }
    public int GetPointsFromParadeRoute()
    {
        int points = 0;
        foreach (Entertainment entertainment in _entertainments)
        {
            if (entertainment.Data.Type == EntertainmentType.ParadeRoute)
            {
                points += entertainment.Points;
            }
        }
        return points;
    }
    public int GetPointsFromMysticGarden()
    {
        int points = 0;
        foreach (Entertainment entertainment in _entertainments)
        {
            if (entertainment.Data.Type == EntertainmentType.MysticGarden)
            {
                points += entertainment.Points;
            }
        }
        return points;
    }
    #endregion

    #region EVENTS
    public event Action<Entertainment> OnEntertainmentSpawned;
    public event Action<EntertainmentData, Tile> OnEntertainmentRemoved;
    public event Action OnScoreUpdated;
    public event Action<Tile, int> OnScoreGained;
    public Action<Tile, int> OnScoreLost;//Directly called by Entertainment when it lose points (or by the manager on destroy)
    //Tutorial event
    public event Action OnClaimedTileSelected;
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnEntertainmentPhaseStarted += StartPhase;
        GameManager.Instance.OnEntertainmentPhaseEnded += ConfirmPhase;
        GameManager.Instance.OnNewTileSelected += NewTileSelected;
        GameManager.Instance.OnTileUnselected += TileUnselected;
    }

    private void Update()
    {
        /*foreach (var kvp in _groupBoost)
        {
            string s = $"Group {kvp.Key}: ";
            foreach (var entertainment in kvp.Value)
            {
                if (!entertainment)
                    continue;
                s += entertainment.name + ", ";
            }
            Debug.Log(s + " Count : " + _groupBoostCount[kvp.Key]);
        }*/
    }

    #region PHASE LOGIC
    protected override void StartPhase()
    {
        GameManager.Instance.UnselectTile();

        //Convert savings into carnivalists
        int goldCarnivalist = ResourcesManager.Instance.GetResourceStock(Resource.Gold) / _goldForOneCarnivalist;
        int srCarnivalist = ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources) / _SrForOneCarnivalist;

        ResourcesManager.Instance.SpendConvertedResources(goldCarnivalist * _goldForOneCarnivalist, srCarnivalist * _SrForOneCarnivalist);

        ResourcesManager.Instance.UpdateCarnivalist(goldCarnivalist + srCarnivalist, Transaction.Gain);
        ResourcesManager.Instance.UpdateCarnivalistSource(_emptyDataForString, goldCarnivalist + srCarnivalist, Transaction.Gain);

        if (!UIManager.Instance.AreEntPlacementShown)
            UIManager.Instance.SwitchEntPlacementVisibility();

        if (UIManager.Instance.AreIncomesShown)
        {
            foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
            {
                tile.ShowIncomeUI(true);//Refresh the income UI if needed to only show the score income
            }
        }
    }

    protected override void ConfirmPhase()
    {
        GameManager.Instance.UnselectTile();

        StartCoroutine(PhaseFinalized());
    }
    #endregion

    //Handle the tile selection action
    protected override void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Entertain)
            return;

        _interactionPositions.Clear();

        if (tile.Claimed)
        {
            OnClaimedTileSelected?.Invoke();

            //Interaction depend on if the tile got an entertainment or not
            if (tile.Entertainment != null)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                DestroyInteraction(tile, 0);
                return;
            }
            else
            {
                if (tile.CanReceiveEntertainment())
                {
                    if (!tile.AllowEntertainment)
                    {
                        _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                        EntertainmentInteraction(tile, 0, _minstrelData);
                    }
                    else
                    {
                        _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, _entertainmentsData.Count);
                        for (int i = 0; i < _entertainmentsData.Count; i++)
                        {
                            EntertainmentInteraction(tile, i, _entertainmentsData[i]);
                        }
                    }
                }
            }
        }
    }

    #region INTERACTION
    private void EntertainmentInteraction(Tile tile, int positionIndex, EntertainmentData data)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Entertainment, null, data));
    }

    private void DestroyInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Destroy));
    }

    public void SpawnEntertainment(Tile tile, EntertainmentData data, bool isPredictionRelated = false)
    {
        if (tile.Entertainment != null)
            return;

        if (ResourcesManager.Instance.CanAffordCarnivalist(data.GetActualCarnivalistCost(tile)) || isPredictionRelated)
        {
            if (!isPredictionRelated)
                ResourcesManager.Instance.UpdateCarnivalist(data.GetActualCarnivalistCost(tile), Transaction.Spent);

            Entertainment currentEntertainment = Instantiate(_entertainmentPrefab,
                tile.transform.position + _entertainmentPrefab.transform.localPosition,
                _entertainmentPrefab.transform.rotation,
                _entertainmentsParent).GetComponent<Entertainment>();

            _entertainments.Add(currentEntertainment);
            currentEntertainment.Initialize(tile, data);
            tile.Entertainment = currentEntertainment;
            OnEntertainmentSpawned?.Invoke(currentEntertainment);
        }
    }

    public void DestroyEntertainment(Tile tile, bool isPredictionRelated = false)
    {
        EntertainmentData removedEntertainmentData = tile.Entertainment.Data;
        if (!isPredictionRelated)
            OnScoreLost?.Invoke(tile, tile.Entertainment.Points);
        tile.Entertainment.DestroyEntertainment();
        _entertainments.Remove(tile.Entertainment);
        tile.Entertainment = null;
        OnEntertainmentRemoved?.Invoke(removedEntertainmentData, tile);
        //Call the check empty group after the Entertainment assignation, so the event and its listener is done before
        CheckEmptyGroup(tile);
        //Refund
        if (!isPredictionRelated)
            ResourcesManager.Instance.UpdateCarnivalist(removedEntertainmentData.GetActualCarnivalistCost(tile), Transaction.Gain);
    }

    public void PredictEntertainmentSpawn(Tile tile, EntertainmentData data,
        out int predictedPoints, out Dictionary<Tile, int> predictedExtSources, out Dictionary<Tile, int> predictedIntSources, out int predictedSelfPoints, 
        out Dictionary<Tile, int> predictedEntImpactedByEnt, out Dictionary<Tile, int> predictedEntImpactedByTile)
    {
        // If there is already an entertainment, no prediction possible (possible is we spawn an entertainment right before calling this function)
        if (tile.Entertainment != null)
        {
            predictedPoints = 0;
            predictedExtSources = new Dictionary<Tile, int>();
            predictedIntSources = new Dictionary<Tile, int>();
            predictedSelfPoints = 0;
            predictedEntImpactedByEnt = new Dictionary<Tile, int>();
            predictedEntImpactedByTile = new Dictionary<Tile, int>();
            return;
        }

        if (_resetPredictBoolCoroutine != null)
            StopCoroutine(_resetPredictBoolCoroutine);
        _isPredictingPoints = true;
        SpawnEntertainment(tile, data, true);
        predictedPoints = tile.Entertainment.Points;
        predictedExtSources = new Dictionary<Tile, int>(tile.Entertainment.ExternalPointsSources);
        predictedIntSources = new Dictionary<Tile, int>(tile.Entertainment.InternalPointsSources);
        predictedSelfPoints = tile.Entertainment.GetPointsFromEntertainmentOnly();
        predictedEntImpactedByEnt = new Dictionary<Tile, int>(tile.EntImpactedByEntertainment);
        predictedEntImpactedByTile = new Dictionary<Tile, int>(tile.EntImpactedByTile);
        DestroyEntertainment(tile, true);
        _resetPredictBoolCoroutine = StartCoroutine(ResetPredictionBool());
    }

    private IEnumerator ResetPredictionBool()
    {
        //Wait end of frame to avoid issues with LateUpdate in Entertainment
        yield return new WaitForEndOfFrame();
        _isPredictingPoints = false;
    }
    #endregion

    public void UpdateScore(int value, Transaction transaction, Tile tile = null, bool skipVFX = false)
    {
        if (transaction == Transaction.Spent)
            value = -value;

        _score += value;

        if (_isPredictingPoints)
            return;

        if (!skipVFX)
        {
            //Play VFX if we gain score
            if (tile != null && transaction == Transaction.Gain)
                OnScoreGained?.Invoke(tile, value);
        }

        OnScoreUpdated?.Invoke();
    }

    //For boostByZone special effect, needed when the group last entry is a BridgeData, otherwise the SO handle everything
    private void CheckEmptyGroup(Tile tile)
    {
        if (tile.GroupID > 0)
        {
            _groupBoost[tile.GroupID].Remove(tile.PreviousEntertainment);

            if (_groupBoost[tile.GroupID].Count == 0)
            {
                _groupBoost.Remove(tile.GroupID);
            }

            tile.GroupID = 0;
        }
    }
}
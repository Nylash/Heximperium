using System;
using System.Collections.Generic;
using UnityEngine;

public class EntertainmentManager : PhaseManager<EntertainmentManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Entertainment Data")]
    [SerializeField] private List<EntertainmentData> _entertainmentsData = new List<EntertainmentData>();
    [SerializeField] private GameObject _entertainmentPrefab;
    [SerializeField] private Transform _entertainmentsParent;
    [Header("_________________________________________________________")]
    [Header("Resources Conversion")]
    [SerializeField] private float _pointForOneGold;
    [SerializeField] private float _pointForOneSR;
    #endregion

    #region VARIABLES
    private List<Entertainment> _entertainments = new List<Entertainment>();
    private int _score;
    private Dictionary<int, List<Entertainment>> _groupBoost = new Dictionary<int, List<Entertainment>>(); //Use for BoostByZoneSize special effect, <GroupID, Entertainments>
    private Dictionary<int, int> _groupBoostCount = new Dictionary<int, int>(); //Use for BoostByZoneSize special effect, <GroupID, Count>
    #endregion

    #region ACCESSORS
    public List<Entertainment> Entertainments { get => _entertainments; }
    public int Score { get => _score; }
    public Dictionary<int, List<Entertainment>> GroupBoost { get => _groupBoost; }
    public Dictionary<int, int> GroupBoostCount { get => _groupBoostCount; }
    #endregion

    #region EVENTS
    public event Action<Entertainment> OnEntertainmentSpawned;
    public event Action OnScoreUpdated;
    public event Action<Tile, int> OnScoreGained;
    public Action<Tile, int> OnScoreLost;//Directly called by Entertainment when it lose points (or by the manager on destroy)
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
        //Convert savings into points
        float goldIntoPoints = _pointForOneGold * ResourcesManager.Instance.GetResourceStock(Resource.Gold);
        float srIntoPoints = _pointForOneSR * ResourcesManager.Instance.GetResourceStock(Resource.SpecialResources);
        _score += Mathf.RoundToInt(goldIntoPoints) + Mathf.RoundToInt(srIntoPoints);
        OnScoreUpdated?.Invoke();
        ResourcesManager.Instance.SpendAllResources();

        //Earn incomes of every claimed tiles
        foreach (Tile tile in ExpansionManager.Instance.ClaimedTiles)
            ResourcesManager.Instance.UpdateResource(tile.Incomes, Transaction.Gain, tile);

        ResourcesManager.Instance.CHEAT_RESOURCES();
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
            //Interaction depend on if the tile got an entertainment or not
            if(tile.Entertainment != null)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                DestroyInteraction(tile, 0);
                return;
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

    #region INTERACTION
    private void EntertainmentInteraction(Tile tile, int positionIndex, EntertainmentData data)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Entertainment, null, data));
    }

    private void DestroyInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Destroy));
    }

    public void SpawnEntertainment(Tile tile, EntertainmentData data)
    {
        if (tile.Entertainment != null)
            return;

        if (ResourcesManager.Instance.CanAfford(data.Costs))
        {
            ResourcesManager.Instance.UpdateResource(data.Costs, Transaction.Spent);

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

    public void DestroyEntertainment(Tile tile)
    {
        OnScoreLost?.Invoke(tile, tile.Entertainment.Points);
        tile.Entertainment.DestroyEntertainment();
        _entertainments.Remove(tile.Entertainment);
        tile.Entertainment = null;
        //Call the check empty group after the Entertainment assignation, so the event and its listener is done before
        CheckEmptyGroup(tile);
    }
    #endregion

    public void UpdateScore(int value, Transaction transaction, Tile tile = null, bool skipVFX = false)
    {
        if (transaction == Transaction.Spent)
            value = -value;

        _score += value;

        if(!skipVFX)
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
                _groupBoostCount.Remove(tile.GroupID);
            }

            tile.GroupID = 0;
        }
    }
}

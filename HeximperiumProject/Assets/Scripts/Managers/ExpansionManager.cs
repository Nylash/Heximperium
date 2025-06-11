using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExpansionManager : Singleton<ExpansionManager>
{
    #region CONSTANTS
    private const string TOWN_DATA_PATH = "Data/Infrastructures/Town";
    #endregion

    #region CONFIGURATION
    [SerializeField] private Transform _borderParent;
    #endregion

    #region VARIABLES
    private List<GameObject> _buttons = new List<GameObject>();
    private List<Tile> _claimedTiles = new List<Tile>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private int _baseClaimPerTurn;
    #endregion

    #region ACCESSORS
    public Transform BorderParent { get => _borderParent; }
    public int BaseClaimPerTurn { get => _baseClaimPerTurn; set => _baseClaimPerTurn = value; }
    public List<Tile> ClaimedTiles { get => _claimedTiles; }
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent OnPhaseFinalized = new UnityEvent();
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnExpansionPhaseStarted.AddListener(StartPhase);
        GameManager.Instance.OnExpansionPhaseEnded.AddListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.AddListener(TileUnselected);
    }

    #region PHASE LOGIC
    private void StartPhase()
    {
        ResourcesManager.Instance.UpdateClaim(_baseClaimPerTurn, Transaction.Gain);
    }

    private void ConfirmPhase()
    {
        ResourcesManager.Instance.UpdateClaim(ResourcesManager.Instance.Claim, Transaction.Spent);

        StartCoroutine(PhaseFinalized());
    }

    private IEnumerator PhaseFinalized()
    {
        // Wait for one frame
        yield return null;

        OnPhaseFinalized.Invoke();
    }
    #endregion

    #region TILE SELECTION
    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Expand)
            return;

        _interactionPositions.Clear();

        //Claimed tiles can only be used for town
        if (tile.Claimed)
        {
            //We can only build town on basic tile
            if (tile.TileData is BasicTileData)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                TownInteraction(tile, 0);
            }
            return;
        }

        _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 2);
        //We can only build town on basic tile
        if (tile.TileData is BasicTileData)
            TownInteraction(tile, 0);
        //We can only claimed tiles adjacent to already claimed tiles
        if (tile.IsOneNeighborClaimed())
            ClaimInteraction(tile, 1);
    }

    private void TileUnselected()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _buttons.Clear();
    }
    #endregion

    #region INTERACTION
    private void ClaimInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Claim));
    }

    private void TownInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Town));
    }

    public void ClaimTile(Tile tile)
    {
        if (tile.Claimed)
            return;
        if (ResourcesManager.Instance.CanAffordClaim(tile.TileData.ClaimCost))
        {
            ResourcesManager.Instance.UpdateClaim(tile.TileData.ClaimCost, Transaction.Spent);
            tile.ClaimTile();
            _claimedTiles.Add(tile);
            foreach (Tile t in _claimedTiles)
                t.CheckBorder();
        }
    }

    public void BuildTown(Tile tile)
    {
        InfrastructureData townData = Resources.Load<InfrastructureData>(TOWN_DATA_PATH);
        if (townData == null)
        {
            Debug.LogError("Town data not found at path: " + TOWN_DATA_PATH);
            return;
        }

        if (ExploitationManager.Instance.IsInfraAvailable(townData))
        {
            if (ResourcesManager.Instance.CanAfford(townData.Costs))
            {
                // Start by claiming the tile if needed
                if (!tile.Claimed)
                {
                    tile.ClaimTile();
                    _claimedTiles.Add(tile);
                    foreach (Tile t in _claimedTiles)
                        t.CheckBorder();
                }
                ExploitationManager.Instance.BuildInfrastructure(tile, townData);
            }
        }
    }

    public void ButtonsFade(bool fade)
    {
        foreach (GameObject item in _buttons)
        {
            item.GetComponent<InteractionButton>().FadeAnimation(fade);
        }
    }
    #endregion
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExpansionManager : PhaseManager<ExpansionManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [SerializeField] private InfrastructureData _townData;
    [SerializeField] private Transform _borderParent;
    [SerializeField] private Transform _claimedTilesParent;
    #endregion

    #region VARIABLES
    private List<Tile> _claimedTiles = new List<Tile>();
    private int _claimPerTurn;
    private int _savedClaimPerTurn;
    //Upgrades variables
    private bool _upgradeTownAutoClaim;
    private bool _upgradeTownsGenerateClaim;
    private bool _upgradeHazardClaimReduction;
    private bool _upgradeClaimRange;
    #endregion

    #region ACCESSORS
    public Transform BorderParent { get => _borderParent; }
    public int ClaimPerTurn { get => _claimPerTurn; set => _claimPerTurn = value; }
    public List<Tile> ClaimedTiles { get => _claimedTiles; }
    public InfrastructureData TownData { get => _townData;}
    public int SavedClaimPerTurn { get => _savedClaimPerTurn; set => _savedClaimPerTurn = value; }
    public bool UpgradeTownAutoClaim { get => _upgradeTownAutoClaim; set => _upgradeTownAutoClaim = value; }
    public bool UpgradeTownsGenerateClaim { get => _upgradeTownsGenerateClaim; set => _upgradeTownsGenerateClaim = value; }
    public bool UpgradeHazardClaimReduction { get => _upgradeHazardClaimReduction; set => _upgradeHazardClaimReduction = value; }
    public bool UpgradeClaimRange { get => _upgradeClaimRange; set => _upgradeClaimRange = value; }
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<Tile> OnTileClaimed = new UnityEvent<Tile>();
    [HideInInspector] public UnityEvent<int> OnClaimSaved = new UnityEvent<int>();
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnExpansionPhaseStarted.AddListener(StartPhase);
        GameManager.Instance.OnExpansionPhaseEnded.AddListener(ConfirmPhase);
        GameManager.Instance.OnNewTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.OnTileUnselected.AddListener(TileUnselected);
    }

    #region PHASE LOGIC
    protected override void StartPhase()
    {
        ResourcesManager.Instance.UpdateClaim(_claimPerTurn, Transaction.Gain);

        if (_upgradeTownsGenerateClaim)
        {
            foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
            {
                if (tile.TileData is InfrastructureData infra)
                {
                    if (infra.IsTown)
                        ResourcesManager.Instance.UpdateClaim(1, Transaction.Gain, tile);
                }
                else
                {
                    Debug.LogError("TileData is not an InfrastructureData on tile: " + tile.name + " and yet it is in the Infrastructures list.");
                }
            }
        }
    }

    protected override void ConfirmPhase()
    {
        if (_savedClaimPerTurn > 0)
        {
            if(ResourcesManager.Instance.Claim > _savedClaimPerTurn)
                ResourcesManager.Instance.UpdateClaim(ResourcesManager.Instance.Claim - _savedClaimPerTurn, Transaction.Spent);
            OnClaimSaved.Invoke(ResourcesManager.Instance.Claim);
        }
        else
            ResourcesManager.Instance.UpdateClaim(ResourcesManager.Instance.Claim, Transaction.Spent);

        GameManager.Instance.UnselectTile();

        StartCoroutine(PhaseFinalized());
    }
    #endregion

    protected override void NewTileSelected(Tile tile)
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
        //We can only claimed tiles adjacent to already claimed tiles (except if we got the upgrade)
        if (tile.IsOneNeighborClaimed())
            ClaimInteraction(tile, 1);
        else if (_upgradeClaimRange)
        {
            if (tile.IsOneNeighborOfNeighborClaimed())
                ClaimInteraction(tile, 1);
        }
    }

    #region INTERACTION
    private void ClaimInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Claim));
    }

    private void TownInteraction(Tile tile, int positionIndex)
    {
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Infrastructure, _townData));
    }

    public void ClaimTile(Tile tile, bool freeClaim)
    {
        if (tile.Claimed)
            return;

        if (ResourcesManager.Instance.CanAffordClaim(tile.TileData.ClaimCost) || freeClaim)
        {
            if (!freeClaim)
                ResourcesManager.Instance.UpdateClaim(tile.TileData.ClaimCost, Transaction.Spent);
            tile.ClaimTile();
            _claimedTiles.Add(tile);
            tile.transform.parent = _claimedTilesParent;
            OnTileClaimed.Invoke(tile);
        }
    }

    public void BuildTown(Tile tile)
    {
        if (ExploitationManager.Instance.IsInfraAvailable(_townData))
        {
            if (ResourcesManager.Instance.CanAfford(_townData.Costs))
            {
                // Start by claiming the tile if needed
                if (!tile.Claimed)
                    ClaimTile(tile, true);

                ExploitationManager.Instance.BuildInfrastructure(tile, _townData);
                UIManager.Instance.UpdateTownLimit();

                if (_upgradeTownAutoClaim)
                {
                    foreach (Tile neighbor in tile.Neighbors)
                    {
                        if (!neighbor)
                            continue;
                        ClaimTile(neighbor, true);
                    }
                }
            }
        }
    }
    #endregion
}

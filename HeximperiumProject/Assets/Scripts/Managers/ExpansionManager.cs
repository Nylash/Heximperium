using System;
using System.Collections.Generic;
using UnityEngine;

public class ExpansionManager : PhaseManager<ExpansionManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [SerializeField] private InfrastructureData _townData;
    [SerializeField] private Transform _claimedTilesParent;
    #endregion

    #region VARIABLES
    private List<Tile> _claimedTiles = new List<Tile>();
    private int _claimPerTurn;
    //Upgrades variables
    private bool _upgradeTownAutoClaim;
    private bool _upgradeClaimRange;
    private bool _upgradeConserveClaims;
    #endregion

    #region ACCESSORS
    public int ClaimPerTurn { get => _claimPerTurn; set => _claimPerTurn = value; }
    public List<Tile> ClaimedTiles { get => _claimedTiles; }
    public InfrastructureData NewTownData { get => _townData;}
    public bool UpgradeTownAutoClaim { get => _upgradeTownAutoClaim; set => _upgradeTownAutoClaim = value; }
    public bool UpgradeClaimRange { get => _upgradeClaimRange;
        set
        {
            _upgradeClaimRange = value;
            if (GameManager.Instance.CurrentPhase == Phase.Expand)
                UpdateInteractableTiles();
        } 
    }

    public bool UpgradeConserveClaims { get => _upgradeConserveClaims; set => _upgradeConserveClaims = value; }
    #endregion

    #region EVENTS
    public event Action<Tile> OnTileClaimed;
    //Tutorial events
    public event Action OnClaimableTileSelected;
    public event Action OnTownableTileSelected;
    #endregion

    protected override void OnAwake()
    {
        GameManager.Instance.OnExpansionPhaseStarted += StartPhase;
        GameManager.Instance.OnExpansionPhaseEnded += ConfirmPhase;
        GameManager.Instance.OnNewTileSelected += NewTileSelected;
        GameManager.Instance.OnTileUnselected += TileUnselected;
    }

    #region PHASE LOGIC
    protected override void StartPhase()
    {
        GameManager.Instance.UnselectTile();

        ResourcesManager.Instance.UpdateClaim(_claimPerTurn, Transaction.Gain);

        foreach (Tile tile in _claimedTiles)
        {
            if (tile.ClaimIncome > 0)
                ResourcesManager.Instance.UpdateClaim(tile.ClaimIncome, Transaction.Gain);
        }

        UpdateInteractableTiles();
    }

    protected override void ConfirmPhase()
    {
        if (!_upgradeConserveClaims && ResourcesManager.Instance.Claim > 0)
            ResourcesManager.Instance.UpdateClaim(ResourcesManager.Instance.Claim, Transaction.Spent);

        GameManager.Instance.UnselectTile();

        ClearInteractableTiles();

        StartCoroutine(PhaseFinalized());
    }
    #endregion

    protected override void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Expand)
            return;

        _interactionPositions.Clear();

        if (TutorialManager.Instance == null)
        {
            //Claimed tiles can only be used for town
            if (tile.Claimed)
            {
                //We can only build town on basic and resource tile
                if (tile.TileData is BasicTileData || tile.TileData is ResourceTileData)
                {
                    _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                    TownInteraction(tile, 0);
                }
                return;
            }

            _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 2);
            //We can only build town on basic and resource tile
            if (tile.TileData is BasicTileData || tile.TileData is ResourceTileData)
                TownInteraction(tile, 0);
            //We can only claimed tiles adjacent to already claimed tiles (except if we got the upgrade)
            if (tile.IsOneNeighborClaimed())
            {
                if (tile.TileData is not HazardousTileData)
                {
                    ClaimInteraction(tile, 1);
                }
            }
            else if (_upgradeClaimRange)
            {
                if (tile.TileData is not HazardousTileData)
                {
                    if (tile.IsOneNeighborOfNeighborClaimed())
                        ClaimInteraction(tile, 1);
                }
            }
        }
        else
        {
            if (TutorialManager.Instance.IsClaimingTile)
            {
                if (tile.IsOneNeighborClaimed() && !tile.Claimed)
                {
                    if (tile.TileData is not HazardousTileData)
                    {
                        _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                        ClaimInteraction(tile, 0);
                    }
                }
            }
            else if (TutorialManager.Instance.IsBuildingTown)
            {
                //We can only build town on basic and resource tile
                if (tile.TileData is BasicTileData || tile.TileData is ResourceTileData)
                {
                    _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                    TownInteraction(tile, 0);
                }
            }
        }
    }

    #region INTERACTION
    private void ClaimInteraction(Tile tile, int positionIndex)
    {
        OnClaimableTileSelected?.Invoke();
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Claim));
    }

    private void TownInteraction(Tile tile, int positionIndex)
    {
        OnTownableTileSelected?.Invoke();
        _buttons.Add(Utilities.CreateInteractionButton(tile, _interactionPositions[positionIndex], Interaction.Infrastructure, _townData));
    }

    public void ClaimTile(Tile tile, bool freeClaim, bool fromInteraction = false)
    {
        if (tile.Claimed)
            return;
        if (tile.TileData is HazardousTileData)
            return;

        if (ResourcesManager.Instance.CanAffordClaim(tile.TileData.ClaimCost) || freeClaim)
        {
            if (!freeClaim)
                ResourcesManager.Instance.UpdateClaim(tile.TileData.ClaimCost, Transaction.Spent);
            tile.ClaimTile();
            _claimedTiles.Add(tile);
            tile.transform.parent = _claimedTilesParent;
            OnTileClaimed?.Invoke(tile);

            if (fromInteraction)
                UpdateInteractableTiles();
        }
    }

    public void BuildTown(Tile tile, bool fromInteraction = false)
    {
        if (ExploitationManager.Instance.IsInfraAvailable(_townData))
        {
            if (ResourcesManager.Instance.CanAffordClaim(_townData.ClaimCost))
            {
                // Start by claiming the tile if needed
                if (!tile.Claimed)
                    ClaimTile(tile, true);

                ExploitationManager.Instance.BuildInfrastructure(tile, _townData);
                ResourcesManager.Instance.UpdateClaim(_townData.ClaimCost, Transaction.Spent);
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

                if (fromInteraction)
                    UpdateInteractableTiles();
            }
        }
    }
    #endregion

    public override void UpdateInteractableTiles()
    {
        bool townBuildable = false;
        HashSet<Tile> validTiles = new HashSet<Tile>();

        foreach (Tile tile in ExplorationManager.Instance.RevealedTiles)
        {
            if (tile.Claimed)
            {
                if (tile.TileData is not InfrastructureData)
                {
                    if (ResourcesManager.Instance.CanAffordClaim(_townData.ClaimCost) && ExploitationManager.Instance.IsInfraAvailable(_townData))
                        townBuildable = true;
                }
            }
            else
            {
                if (tile.IsOneNeighborClaimed() || (_upgradeClaimRange && tile.IsOneNeighborOfNeighborClaimed()))
                {
                    if (tile.TileData is not HazardousTileData)
                    {
                        if (ResourcesManager.Instance.CanAffordClaim(tile.TileData.ClaimCost))
                        {
                            validTiles.Add(tile);
                        }
                    }
                }
                if (tile.TileData is not HazardousTileData)
                {
                    if (ResourcesManager.Instance.CanAffordClaim(_townData.ClaimCost) && ExploitationManager.Instance.IsInfraAvailable(_townData))
                        townBuildable = true;
                }
            }
        }

        LaunchInteractableTiles(validTiles);
        if (townBuildable)
            UIManager.Instance.BuildTownHint.gameObject.SetActive(true);
        else
        {
            if (UIManager.Instance.BuildTownHint.gameObject.activeSelf)
                UIManager.Instance.BuildTownHint.SetTrigger("Hide");
        }
    }

    protected override void LaunchInteractableTiles(HashSet<Tile> validTiles)
    {
        bool alreadyAffected = false;
        if (_interactibleTiles.Count == 0)
        {
            alreadyAffected = true;
            _interactibleTiles = new HashSet<Tile>(validTiles);
        }

        foreach (Tile tile in validTiles)
            tile.PreviewBorder(true);
        foreach (Tile tile in _interactibleTiles)
        {
            if (!validTiles.Contains(tile))
                tile.PreviewBorder(false);
        }

        if (!alreadyAffected)
            _interactibleTiles = new HashSet<Tile>(validTiles);
    }

    protected override void ClearInteractableTiles()
    {
        foreach (Tile tile in _interactibleTiles)
        {
            tile.PreviewBorder(false);
        }
        _interactibleTiles.Clear();
        if (UIManager.Instance.BuildTownHint.gameObject.activeSelf)
            UIManager.Instance.BuildTownHint.SetTrigger("Hide");
    }
}

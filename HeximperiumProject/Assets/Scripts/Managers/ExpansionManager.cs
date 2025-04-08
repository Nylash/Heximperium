using System.Collections.Generic;
using UnityEngine;

public class ExpansionManager : Singleton<ExpansionManager>
{
    [SerializeField] private Transform _borderParent;

    private List<GameObject> _buttons = new List<GameObject>();
    private List<Tile> _claimedTiles = new List<Tile>();
    private List<Vector3> _interactionPositions = new List<Vector3>();
    private int _availableTown;
    private int _baseClaimPerTurn;

    public int AvailableTown { get => _availableTown; set => _availableTown = value; }
    public Transform BorderParent { get => _borderParent; }
    public int BaseClaimPerTurn { get => _baseClaimPerTurn; set => _baseClaimPerTurn = value; }
    public List<Tile> ClaimedTiles { get => _claimedTiles; }

    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
        GameManager.Instance.event_newTileSelected.AddListener(NewTileSelected);
        GameManager.Instance.event_tileUnselected.AddListener(TileUnselected);
    }

    private void StartPhase(Phase phase)
    {
        if (phase != Phase.Expand)
            return;
        ResourcesManager.Instance.UpdateClaim(_baseClaimPerTurn, Transaction.Gain);
    }

    public void ConfirmingPhase()
    {
        ResourcesManager.Instance.UpdateClaim(ResourcesManager.Instance.Claim, Transaction.Spent);
    }

    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Expand)
            return;

        if (tile.Claimed)
        {
            if(tile.TileData as BasicTileData)
            {
                _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);
                TownInteraction(tile, 0);
            }
            return;
        }


        _interactionPositions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 2);
        if (tile.TileData as BasicTileData)
            TownInteraction(tile, 0);
        if (tile.IsOneNeighborClaimed())
            ClaimInteraction(tile, 1);

        _interactionPositions.Clear();
    }

    public void ClaimTile(Tile tile)
    {
        if (!tile.Claimed)
        {
            if(ResourcesManager.Instance.CanAffordClaim(tile.TileData.ClaimCost))
            {
                ResourcesManager.Instance.UpdateClaim(tile.TileData.ClaimCost, Transaction.Spent);
                tile.ClaimTile();
                _claimedTiles.Add(tile);
                foreach (Tile t in _claimedTiles)
                    t.CheckBorder();
            }
        }
        else
        {
            Debug.LogError("This tile shouldn't have the claim interaction " + tile.name);
        }
    }

    public void BuildTown(Tile tile)
    {
        if (_availableTown != 0)// && ResourcesManager.Instance.CanAfford())
        {
            //Start by claiming the tile if needed
            if (!tile.Claimed)
            {
                tile.ClaimTile();
                _claimedTiles.Add(tile);
                foreach (Tile t in _claimedTiles)
                    t.CheckBorder();
            }
            _availableTown -= 1;
            ExploitationManager.Instance.BuildInfrastructure(tile, Resources.Load<InfrastructureData>("Data/Infrastructures/Town"));
        }
        
    }

    private void TileUnselected()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _buttons.Clear();
    }


    private void ClaimInteraction(Tile tile, int positionIndex)
    {
        GameObject buttonClaim = Instantiate(GameManager.Instance.InteractionPrefab, _interactionPositions[positionIndex], Quaternion.identity);
        buttonClaim.GetComponent<UI_InteractionButton>().Initialize(tile, Interaction.Claim);

        _buttons.Add(buttonClaim);
    }

    private void TownInteraction(Tile tile, int positionIndex)
    {
        GameObject buttonTown = Instantiate(GameManager.Instance.InteractionPrefab, _interactionPositions[positionIndex], Quaternion.identity);
        buttonTown.GetComponent<UI_InteractionButton>().Initialize(tile, Interaction.Town);

        _buttons.Add(buttonTown);
    }
}

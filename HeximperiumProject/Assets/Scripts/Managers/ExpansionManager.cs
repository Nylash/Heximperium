using System.Collections.Generic;
using UnityEngine;

public class ExpansionManager : Singleton<ExpansionManager>
{
    private List<GameObject> _buttons = new List<GameObject>();
    private List<Tile> _claimedTiles = new List<Tile>();

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
        print("Start Expansion");
        ResourcesManager.Instance.Claim += 500;
    }

    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Expand)
            return;

        if (tile.Claimed)
        {
            //if not town or infra or resource or special
            TownInteraction(tile);
            return;
        }
            
        if(tile.IsOneNeighborClaimed())
            ClaimInteraction(tile);
        TownInteraction(tile);
    }

    public void ClaimTile(Tile tile)
    {
        if (!tile.Claimed)
        {
            if(ResourcesManager.Instance.CanAfford(Resource.Claim, tile.TileData.ClaimCost))
            {
                ResourcesManager.Instance.Claim -= tile.TileData.ClaimCost;
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

    private void TileUnselected()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _buttons.Clear();
    }

    private void ClaimInteraction(Tile tile)
    {
        //Get positions for buttons
        List<Vector3> _positions = Utilities.GetInteractionButtonsPosition(tile.transform.position, 1);

        GameObject button = Instantiate(GameManager.Instance.InteractionPrefab, _positions[0], Quaternion.identity);
        button.GetComponent<UI_InteractionButton>().
            Initialize(tile, Interaction.Claim, ResourcesManager.Instance.CanAfford(Resource.Claim, tile.TileData.ClaimCost));

        _buttons.Add(button);
    }

    private void TownInteraction(Tile tile)
    {

    }
}

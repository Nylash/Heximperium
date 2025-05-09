using TMPro;
using UnityEngine;

public class PopUp_InteractionClaim : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _claimCostText;

    public override void InitializePopUp(Tile tile, UI_InteractionButton button = null)
    {
        _claimCostText.text = tile.TileData.ClaimCost.ToString();
    }
}

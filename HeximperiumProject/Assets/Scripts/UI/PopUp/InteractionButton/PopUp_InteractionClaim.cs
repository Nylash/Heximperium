using TMPro;
using UnityEngine;

public class PopUp_InteractionClaim : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _claimCostText;

    public override void InitializePopUp(UI_InteractionButton button)
    {
        _claimCostText.text = button.AssociatedTile.TileData.ClaimCost.ToString();
    }
}

using TMPro;
using UnityEngine;

public class PopUp_TileWithoutGoldIncome : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _claimText;

    public override void InitializePopUp(Tile tile, UI_InteractionButton button = null)
    {
        _nameText.text = tile.TileData.TileName;
        _effectText.text = tile.TileData.TilePopUpText;

        if (tile.Claimed)
            _claimText.text = "Tile claimed";
        else
            _claimText.text = "Claim cost : " + tile.TileData.ClaimCost.ToString();
    }
}

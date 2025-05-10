using TMPro;
using UnityEngine;

public class PopUp_TileWithoutGoldIncome : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _claimText;

    public override void InitializePopUp(Tile tile)
    {
        _nameText.text = tile.TileData.TileName;
        _effectText.text = tile.TileData.TilePopUpText;

        if (tile.Claimed)
            _claimText.text = "Tile claimed";
        else
            _claimText.text = "Claim cost : " + tile.TileData.ClaimCost.ToString();
    }
}

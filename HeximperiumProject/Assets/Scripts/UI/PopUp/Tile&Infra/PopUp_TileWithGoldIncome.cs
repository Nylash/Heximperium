using UnityEngine;
using TMPro;

public class PopUp_TileWithGoldIncome : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _claimText;

    public override void InitializePopUp<T>(T item)
    {
        if (item is Tile tile)
        {
            InitializePopUp(tile);
        }
    }

    private void InitializePopUp(Tile tile)
    {
        _nameText.text = tile.TileData.TileName;
        _effectText.text = tile.TileData.TilePopUpText;
        _goldText.text = tile.TileData.GetSpecificIncome(Resource.Gold).ToString();

        if (tile.Claimed)
            _claimText.text = "Tile claimed";
        else
            _claimText.text = "Claim cost : " + tile.TileData.ClaimCost.ToString();
    }
}

using TMPro;
using UnityEngine;

public class PopUp_InfraEffectAndIncome : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _previousTile;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _incomeText;

    public override void InitializePopUp(Tile tile, UI_InteractionButton button = null)
    {
        _nameText.text = tile.TileData.TileName;
        _previousTile.text = tile.InitialData.TileName;
        _effectText.text = tile.TileData.TilePopUpText;

        if (tile.Incomes.Count > 1)
            Debug.LogError("This tile doesn't show the right pop up.");

        _incomeText.text = tile.Incomes[0].resource.ToString() + " income : " + tile.Incomes[0].value.ToString();
    }
}

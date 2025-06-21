using TMPro;
using UnityEngine;

public class PopUp_InfraIncomeOnly : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _previousTile;
    [SerializeField] private TextMeshProUGUI _incomeText;

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
        _previousTile.text = tile.InitialData.TileName;

        if (tile.Incomes.Count > 1)
            Debug.LogWarning("This tile doesn't show the right pop up.");

        _incomeText.text = tile.Incomes[0].resource.ToCustomString() + " income : +" + tile.Incomes[0].value;
    }
}

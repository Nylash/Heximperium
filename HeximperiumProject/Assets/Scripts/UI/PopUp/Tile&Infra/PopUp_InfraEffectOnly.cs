using TMPro;
using UnityEngine;

public class PopUp_InfraEffectOnly : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _previousTile;
    [SerializeField] private TextMeshProUGUI _effectText;

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
        _effectText.text = tile.TileData.TilePopUpText;
    }
}

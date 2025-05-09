using TMPro;
using UnityEngine;

public class PopUp_InfraEffectOnly : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _previousTile;
    [SerializeField] private TextMeshProUGUI _effectText;

    public override void InitializePopUp(Tile tile, UI_InteractionButton button = null)
    {
        _nameText.text = tile.TileData.TileName;
        _previousTile.text = tile.InitialData.TileName;
        _effectText.text = tile.TileData.TilePopUpText;
    }
}

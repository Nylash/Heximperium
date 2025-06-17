using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;

public class PopUp_InfraEffectAndIncome : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _previousTile;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _incomeText;

    private Tile _associatedTile;

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

        if (tile.Incomes.Count > 1)
            Debug.LogError("This tile doesn't show the right pop up.");

        if(tile.Incomes.Count == 0)
        {
            _incomeText.text = "Income : +0";
        }
        else
        {
            _incomeText.text = tile.Incomes[0].resource.ToCustomString() + " income : +" + tile.Incomes[0].value;
        }

        //Show highlight impacted tiles by the special behaviour of this infra
        if (tile.TileData.SpecialBehaviours.Count != 0)
        {
            foreach (SpecialBehaviour item in tile.TileData.SpecialBehaviours)
            {
                item.HighlightImpactedTile(tile, true);
            }
        }

        _associatedTile = tile;
    }

    public override void DestroyPopUp()
    {
        //Hide highlight impacted tiles by the special behaviour of this infra
        if (_associatedTile.TileData.SpecialBehaviours.Count != 0)
        {
            foreach (SpecialBehaviour item in _associatedTile.TileData.SpecialBehaviours)
            {
                item.HighlightImpactedTile(_associatedTile, false);
            }
        }

        base.DestroyPopUp();
    }
}

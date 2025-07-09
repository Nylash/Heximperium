using TMPro;
using UnityEngine;

public class PopUp_EntertainmentOnTile : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _pointsText;

    Entertainment _associatedEntertainment;

    public override void InitializePopUp<T>(T item)
    {
        if (item is Entertainment entertainment)
        {
            InitializePopUp(entertainment);
        }
    }

    private void InitializePopUp(Entertainment entertainment)
    {
        _nameText.text = entertainment.Data.Type.ToCustomString();
        _pointsText.text = entertainment.Points.ToString();

        if (entertainment.Data.SpecialEffect != null)
            entertainment.Data.SpecialEffect.HighlightImpactedEntertainment(entertainment.Tile, true);

        _associatedEntertainment = entertainment;
    }

    public override void DestroyPopUp()
    {
        if (_associatedEntertainment.Data.SpecialEffect != null)
            _associatedEntertainment.Data.SpecialEffect.HighlightImpactedEntertainment(_associatedEntertainment.Tile, false);

        base.DestroyPopUp();
    }
}

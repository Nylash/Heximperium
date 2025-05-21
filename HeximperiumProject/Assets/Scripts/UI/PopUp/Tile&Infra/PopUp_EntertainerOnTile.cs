using TMPro;
using UnityEngine;

public class PopUp_EntertainerOnTile : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _familyText;
    [SerializeField] private TextMeshProUGUI _synergyText;

    private Entertainer _entertainer;

    public override void InitializePopUp<T>(T item)
    {
        if (item is Entertainer unit)
        {
            InitializePopUp(unit);
        }
    }

    private void InitializePopUp(Entertainer unit)
    {
        _nameText.text = unit.EntertainerData.EntertainerType.ToCustomString();
        _pointsText.text += unit.Points;
        _familyText.text += unit.EntertainerData.Family;
        _synergyText.text += unit.EntertainerData.Synergies[0].ToCustomString() + " & " + unit.EntertainerData.Synergies[1].ToCustomString();

        unit.EntertainerData.HighlightSynergyTile(unit.Tile, true);

        _entertainer = unit;
    }

    public override void DestroyPopUp()
    {
        _entertainer.EntertainerData.HighlightSynergyTile(_entertainer.Tile, false);

        base.DestroyPopUp();
    }
}

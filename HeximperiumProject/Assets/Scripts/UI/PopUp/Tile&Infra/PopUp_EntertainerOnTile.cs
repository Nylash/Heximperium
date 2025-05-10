using TMPro;
using UnityEngine;

public class PopUp_EntertainerOnTile : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _familyText;
    [SerializeField] private TextMeshProUGUI _synergyText;

    public override void InitializePopUp(Entertainer unit)
    {
        _nameText.text = unit.EntertainerData.EntertainerType.ToCustomString();
        _pointsText.text += unit.EntertainerData.Points;
        _familyText.text += unit.EntertainerData.Family;
        _synergyText.text += unit.EntertainerData.Synergies[0].ToCustomString() + " & " + unit.EntertainerData.Synergies[1].ToCustomString();
    }
}

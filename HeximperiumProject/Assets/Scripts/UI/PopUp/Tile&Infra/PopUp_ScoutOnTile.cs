using TMPro;
using UnityEngine;

public class PopUp_ScoutOnTile : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _directionText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _radiusText;
    [SerializeField] private TextMeshProUGUI _lifespanText;

    public override void InitializePopUp<T>(T item)
    {
        if (item is Scout scout)
        {
            InitializePopUp(scout);
        }
    }

    private void InitializePopUp(Scout unit)
    {
        _directionText.text = unit.Direction.ToCustomString();
        _speedText.text += unit.Speed;
        _radiusText.text += unit.RevealRadius;
        _lifespanText.text += unit.Lifespan;
    }
}

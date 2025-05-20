using TMPro;
using UnityEngine;

public class PopUp_Points : UI_ResourcePopUp
{
    [SerializeField] private TextMeshProUGUI _pointsCount;
    [SerializeField] private TextMeshProUGUI _pointsIncome;

    public override void InitializePopUp()
    {
        _pointsCount.text += EntertainementManager.Instance.Score;
        _pointsIncome.text += "+" + EntertainementManager.Instance.GetPointsIncome();
    }
}

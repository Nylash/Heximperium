using TMPro;
using UnityEngine;

public class PopUp_Points : UI_ResourcePopUp
{
    [SerializeField] private TextMeshProUGUI _pointsCount;
    [SerializeField] private TextMeshProUGUI _pointsIncome;

    public override void InitializePopUp()
    {
        _pointsCount.text += EntertainmentManager.Instance.Score;
        _pointsIncome.text += "+" + EntertainmentManager.Instance.Score;
    }
}

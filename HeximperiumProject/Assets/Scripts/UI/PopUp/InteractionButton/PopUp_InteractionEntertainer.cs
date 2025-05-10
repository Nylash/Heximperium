using TMPro;
using UnityEngine;

public class PopUp_InteractionEntertainer : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _familyText;
    [SerializeField] private TextMeshProUGUI _synergyText;
    [SerializeField] private TextMeshProUGUI _costText;

    public override void InitializePopUp(UI_InteractionButton button)
    {
        if (button.UnitData is EntertainerData data)
        {
            _effectText.text += data.EntertainerType.ToCustomString();
            _pointsText.text += data.Points;
            _familyText.text += data.Family;
            _synergyText.text += data.Synergies[0].ToCustomString() + " & " + data.Synergies[1].ToCustomString();
            if (!ResourcesManager.Instance.CanAfford(data.Costs))
                _costText.color = UIManager.Instance.ColorCantAfford;
            for (int i = 0; i < data.Costs.Count; i++)
            {
                if(i == 0)
                    _costText.text += data.Costs[i].value + " " + data.Costs[i].resource.ToCustomString();
                else
                    _costText.text += " & " + data.Costs[i].value + " " + data.Costs[i].resource.ToCustomString();
            }
        }
        else
        {
            Debug.LogError("UnitData is not of type EntertainerData");
        }
    }
}

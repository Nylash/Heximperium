using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PopUp_InteractionScout : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _radiusText;
    [SerializeField] private TextMeshProUGUI _lifespanText;
    [SerializeField] private TextMeshProUGUI _costText;

    public override void InitializePopUp(Tile tile, UI_InteractionButton button = null)
    {
        if (button.UnitData is ScoutData scoutData)
        {
            _speedText.text += scoutData.Speed;
            _radiusText.text += scoutData.RevealRadius;
            _lifespanText.text += scoutData.Lifespan + " turns";
            if (ExplorationManager.Instance.FreeScouts > 0)
                _costText.text = "Remaining free scouts : " + ExplorationManager.Instance.FreeScouts;
            else
            {
                if (!ResourcesManager.Instance.CanAfford(scoutData.Costs))
                    _costText.color = UIManager.Instance.ColorCantAfford;
                _costText.text = "Cost : " + scoutData.Costs[0].value + " " + scoutData.Costs[0].resource.ToString();
            }
                
        }
        else
        {
            Debug.LogError("UnitData is not of type ScoutData");
        }
    }
}

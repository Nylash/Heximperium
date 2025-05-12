using TMPro;
using UnityEngine;

public class PopUp_InteractionInfra : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _copyText;
    [SerializeField] private TextMeshProUGUI _detailsText;
    [SerializeField] private TextMeshProUGUI _costText;

    public override void InitializePopUp<T>(T item)
    {
        if (item is UI_InteractionButton button)
        {
            InitializePopUp(button);
        }
    }

    private void InitializePopUp(UI_InteractionButton button)
    {
        _effectText.text += button.InfrastructureData.TileName;
        _detailsText.text = button.InfrastructureData.InteractionButtonPopUpText;

        if (!ResourcesManager.Instance.CanAfford(button.InfrastructureData.Costs))
            _costText.color = UIManager.Instance.ColorCantAfford;
        for (int i = 0; i < button.InfrastructureData.Costs.Count; i++)
        {
            if (i == 0)
                _costText.text += button.InfrastructureData.Costs[i].value + " " + button.InfrastructureData.Costs[i].resource.ToCustomString();
            else
                _costText.text += " & " + button.InfrastructureData.Costs[i].value + " " + button.InfrastructureData.Costs[i].resource.ToCustomString();
        }

        //Check if the infra has copy limitation
        foreach (InfraAvailableCopy item in ExploitationManager.Instance.InfraAvailableCopies)
        {
            if (button.InfrastructureData == item.infrastructure)
            {
                if(item.availableCopy == 0)
                    _copyText.color = UIManager.Instance.ColorCantAfford;

                _copyText.text += item.availableCopy;
                return;
            }
        }

        //Infra has no copy limitation so we hide the text associated
        _copyText.enabled = false;
    }
}

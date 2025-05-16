using TMPro;
using UnityEngine;

public class PopUp_InteractionInfra : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _copyText;
    [SerializeField] private TextMeshProUGUI _detailsText;
    [SerializeField] private TextMeshProUGUI _costText;

    private UI_InteractionButton _associatedButton;

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
                _copyText.enabled = true;
                break;
            }
        }

        //Show highlight impacted tiles by the special behaviour of this infra
        if(button.InfrastructureData.SpecialBehaviour != null)
        {
            button.InfrastructureData.SpecialBehaviour.HighlightImpactedTile(button.AssociatedTile ,true);
        }

        //Fade out interaction buttons and spawn a clone on the tile where the interaction will be
        GameManager.Instance.InteractionButtonsFade(true);
        button.CreateHighlightedClone();

        _associatedButton = button;
    }

    public override void DestroyPopUp()
    {
        //Hide highlight impacted tiles by the special behaviour of this infra
        if (_associatedButton.InfrastructureData.SpecialBehaviour != null)
        {
            _associatedButton.InfrastructureData.SpecialBehaviour.HighlightImpactedTile(_associatedButton.AssociatedTile, false); 
        }

        //Fade in interaction buttons and remove the clone
        GameManager.Instance.InteractionButtonsFade(false);
        _associatedButton.DestroyHighlightedClone();

        base.DestroyPopUp();
    }
}

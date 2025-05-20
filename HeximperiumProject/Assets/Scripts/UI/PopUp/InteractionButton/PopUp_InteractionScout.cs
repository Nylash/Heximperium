using TMPro;
using UnityEngine;

public class PopUp_InteractionScout : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _radiusText;
    [SerializeField] private TextMeshProUGUI _lifespanText;
    [SerializeField] private TextMeshProUGUI _costText;

    private InteractionButton _associatedButton;

    public override void InitializePopUp<T>(T item)
    {
        if (item is InteractionButton button)
        {
            InitializePopUp(button);
        }
    }

    private void InitializePopUp(InteractionButton button)
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
                _costText.text = "Cost : " + scoutData.Costs[0].value + " " + scoutData.Costs[0].resource.ToCustomString();
            }
                
        }
        else
        {
            Debug.LogError("UnitData is not of type ScoutData");
        }

        //Fade out interaction buttons and spawn a clone on the tile where the interaction will be
        GameManager.Instance.InteractionButtonsFade(true);
        button.CreateHighlightedClone();

        _associatedButton = button;
    }

    public override void DestroyPopUp()
    {
        //Fade in interaction buttons and remove the clone
        GameManager.Instance.InteractionButtonsFade(false);
        _associatedButton.DestroyHighlightedClone();

        base.DestroyPopUp();
    }
}

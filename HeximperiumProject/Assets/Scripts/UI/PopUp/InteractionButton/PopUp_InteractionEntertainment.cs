using TMPro;
using UnityEngine;

public class PopUp_InteractionEntertainment : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _pointsText;
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
        if (button.EntertainData != null) 
        {
            _effectText.text = button.EntertainData.Type.ToCustomString();
            _pointsText.text = button.EntertainData.BasePoints.ToString();

            if (!ResourcesManager.Instance.CanAfford(button.EntertainData.Costs))
                _costText.color = UIManager.Instance.ColorCantAfford;
            for (int i = 0; i < button.EntertainData.Costs.Count; i++)
            {
                if (i == 0)
                    _costText.text += button.EntertainData.Costs[i].value + " " + button.EntertainData.Costs[i].resource.ToCustomString();
                else
                    _costText.text += " & " + button.EntertainData.Costs[i].value + " " + button.EntertainData.Costs[i].resource.ToCustomString();
            }

            if (button.EntertainData.SpecialEffect != null)
                button.EntertainData.SpecialEffect.HighlightImpactedEntertainment(button.AssociatedTile, true);
        }

        //Fade out interaction buttons and spawn a clone on the tile where the interaction will be
        GameManager.Instance.InteractionButtonsFade(true);
        button.CreateHighlightedClone();

        _associatedButton = button;
    }

    public override void DestroyPopUp()
    {
        if (_associatedButton.EntertainData != null)
        {
            if (_associatedButton.EntertainData.SpecialEffect != null)
                _associatedButton.EntertainData.SpecialEffect.HighlightImpactedEntertainment(_associatedButton.AssociatedTile, false);
        }

        //Fade in interaction buttons and remove the clone
        GameManager.Instance.InteractionButtonsFade(false);
        _associatedButton.DestroyHighlightedClone();

        base.DestroyPopUp();
    }
}

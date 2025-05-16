using TMPro;
using UnityEngine;

public class PopUp_InteractionEntertainer : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _familyText;
    [SerializeField] private TextMeshProUGUI _synergyText;
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

using TMPro;
using UnityEngine;

public class PopUp_InteractionScout : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _radiusText;
    [SerializeField] private TextMeshProUGUI _lifespanText;
    [SerializeField] private TextMeshProUGUI _limitText;

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
        _speedText.text += button.ScoutData.Speed;
        _radiusText.text += button.ScoutData.RevealRadius;
        _lifespanText.text += button.ScoutData.Lifespan + " turns";
        if (ExplorationManager.Instance.CurrentScoutsCount >= ExplorationManager.Instance.ScoutsLimit)
            _limitText.color = UIManager.Instance.ColorCantAfford;
        _limitText.text += ExplorationManager.Instance.CurrentScoutsCount + "/" + ExplorationManager.Instance.ScoutsLimit;

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

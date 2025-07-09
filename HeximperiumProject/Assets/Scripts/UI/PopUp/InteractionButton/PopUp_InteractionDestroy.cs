using TMPro;
using UnityEngine;

public class PopUp_InteractionDestroy : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;

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
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                Debug.LogError("There shouldn't be Destroy interaction on explore phase.");
                break;
            case Phase.Expand:
                Debug.LogError("There shouldn't be Destroy interaction on explore phase.");
                break;
            case Phase.Exploit:
                _effectText.text = "Destroy the infrastructure";
                break;
            case Phase.Entertain:
                _effectText.text = "Destroy the entertainment";
                break;
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

using TMPro;
using UnityEngine;

public class PopUp_InteractionDestroy : UI_PopUp
{
    [SerializeField] private TextMeshProUGUI _effectText;

    public override void InitializePopUp<T>(T item)
    {
        if (item is UI_InteractionButton button)
        {
            InitializePopUp(button);
        }
    }

    private void InitializePopUp(UI_InteractionButton button)
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
                _effectText.text = "Destroy the entertainer";
                break;
        }
    }
}

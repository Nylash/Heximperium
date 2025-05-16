using TMPro;
using UnityEngine;

public class PopUp_InteractionClaim : UI_DynamicPopUp
{
    [SerializeField] private TextMeshProUGUI _claimCostText;

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
        _claimCostText.text = button.AssociatedTile.TileData.ClaimCost.ToString();

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

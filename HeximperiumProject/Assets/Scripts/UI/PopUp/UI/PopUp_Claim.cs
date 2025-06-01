using TMPro;
using UnityEngine;

public class PopUp_Claim : UI_ResourcePopUp
{
    [SerializeField] private TextMeshProUGUI _claimCount;
    [SerializeField] private TextMeshProUGUI _claimSource;

    public override void InitializePopUp()
    {
        _claimCount.text += ResourcesManager.Instance.Claim;
        _claimSource.text += "+" + ExpansionManager.Instance.BaseClaimPerTurn;
    }
}

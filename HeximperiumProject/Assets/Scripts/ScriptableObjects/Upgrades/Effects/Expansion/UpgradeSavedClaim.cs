using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UpgradeSavedClaim")]
public class UpgradeSavedClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.SavedClaimPerTurn++;
    }

    public override string GetEffectDescription()
    {
        return "+1 stockable <sprite name=\"Claim_Emoji\">";
    }
}

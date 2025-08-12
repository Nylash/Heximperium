using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UnlockHazardClaimReduction")]
public class UnlockHazardClaimReduction : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeHazardClaimReduction = true;
    }

    public override string GetEffectDescription()
    {
        return "Hazardous tiles cost only 1<sprite name=\"Claim_Emoji\"> to acquire";
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Expansion/UnlockClaimRangeImproved")]
public class UnlockClaimRangeImproved : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeClaimRange = true;
    }

    public override string GetEffectDescription()
    {
        return "Allow claiming<sprite name=\"Claim_Emoji\"> tiles that are up to 1 tile away from your current territory";
    }
}

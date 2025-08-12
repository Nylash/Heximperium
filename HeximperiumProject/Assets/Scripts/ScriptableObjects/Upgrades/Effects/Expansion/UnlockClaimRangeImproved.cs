using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UnlockClaimRangeImproved")]
public class UnlockClaimRangeImproved : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeClaimRange = true;
    }

    public override string GetEffectDescription()
    {
        return "Allow claiming tiles that are up to 1 tile away from your current territory";
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UnlockTownGenerateClaim")]
public class UnlockTownGenerateClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeTownsGenerateClaim = true;
    }

    public override string GetEffectDescription()
    {
        return "Each Town generates +1<sprite name=\"Claim_Emoji\"> per turn";
    }
}

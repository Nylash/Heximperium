using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UpgradeClaim")]
public class UpgradeClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.ClaimPerTurn++;
    }

    public override string GetEffectDescription()
    {
        return "+1<sprite name=\"Claim_Emoji\"> per turn";
    }
}

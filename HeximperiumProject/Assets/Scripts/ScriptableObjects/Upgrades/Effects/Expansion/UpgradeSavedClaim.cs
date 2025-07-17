using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UpgradeSavedClaim")]
public class UpgradeSavedClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.SavedClaimPerTurn++;
    }
}

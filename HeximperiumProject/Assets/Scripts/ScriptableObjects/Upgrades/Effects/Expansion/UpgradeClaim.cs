using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UpgradeClaim")]
public class UpgradeClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.ClaimPerTurn++;
    }
}

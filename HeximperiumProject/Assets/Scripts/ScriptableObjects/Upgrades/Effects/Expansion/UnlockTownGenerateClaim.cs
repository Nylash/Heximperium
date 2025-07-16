using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UnlockTownGenerateClaim")]
public class UnlockTownGenerateClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeTownsGenerateClaim = true;
    }
}

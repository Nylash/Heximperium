using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Expansion/UnlockSavingClaims")]
public class UnlockSavingClaims : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeConserveClaims = true;
    }

    public override string GetEffectDescription()
    {
        return "Claims are now conserved between turns";
    }
}

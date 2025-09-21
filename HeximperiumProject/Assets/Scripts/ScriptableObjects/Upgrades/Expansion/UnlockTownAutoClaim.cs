using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UnlockTownAutoClaim")]
public class UnlockTownAutoClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeTownAutoClaim = true;
    }

    public override string GetEffectDescription()
    {
        return "Founding a Town automatically claim the 6 surrounding tiles";
    }
}

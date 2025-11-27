using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Expansion/UnlockTownAutoClaim")]
public class UnlockTownAutoClaim : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExpansionManager.Instance.UpgradeTownAutoClaim = true;
    }

    public override string GetEffectDescription()
    {
        return "Founding a New Town<sprite name=\"Town_Emoji\"> automatically claims<sprite name=\"Claim_Emoji\"> the 6 surrounding tiles";
    }
}

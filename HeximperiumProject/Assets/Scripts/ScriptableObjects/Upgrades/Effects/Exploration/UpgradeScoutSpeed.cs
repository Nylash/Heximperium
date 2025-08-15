using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UpgradeScoutSpeed")]
public class UpgradeScoutSpeed : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.BoostScoutSpeed++;
    }

    public override string GetEffectDescription()
    {
        return "+1 to Scouts' speed";
    }
}

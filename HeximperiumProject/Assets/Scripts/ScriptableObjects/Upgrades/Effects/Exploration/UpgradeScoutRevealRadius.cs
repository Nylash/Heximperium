using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UpgradeScoutRevealRadius")]
public class UpgradeScoutRevealRadius : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.BoostScoutRevealRadius++;
    }

    public override string GetEffectDescription()
    {
        return "+1 to Scouts' reveal radius";
    }
}

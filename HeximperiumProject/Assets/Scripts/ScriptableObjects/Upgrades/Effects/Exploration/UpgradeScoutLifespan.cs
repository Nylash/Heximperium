using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UpgradeScoutLifespan")]
public class UpgradeScoutLifespan : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.BoostScoutLifespan++;
    }

    public override string GetEffectDescription()
    {
        return "+1 to Scouts' lifespan";
    }
}

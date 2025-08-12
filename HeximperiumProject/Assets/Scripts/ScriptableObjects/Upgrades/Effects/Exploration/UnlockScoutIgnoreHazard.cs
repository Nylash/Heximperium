using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockScoutIgnoreHazard")]
public class UnlockScoutIgnoreHazard : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutIgnoreHazard = true;
    }

    public override string GetEffectDescription()
    {
        return "Scouts ignore slow penalty from Hazardous tiles (Mountain, Desert, Swamp & Water)";
    }
}

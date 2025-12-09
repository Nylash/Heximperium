using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Exploration/UnlockScoutIgnoreHazard")]
public class UnlockScoutIgnoreHazard : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutIgnoreHazard = true;
    }

    public override string GetEffectDescription()
    {
        return "Scouts<sprite name=\"Scout_Emoji\"> ignore slow penalty from Mountain, Desert, Swamp and Water tiles";
    }
}

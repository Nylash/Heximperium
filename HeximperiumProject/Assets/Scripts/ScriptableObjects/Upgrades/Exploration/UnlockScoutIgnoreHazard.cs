using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Exploration/UnlockScoutIgnoreHazard")]
public class UnlockScoutIgnoreHazard : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutIgnoreHazard = true;
        ExplorationManager.Instance.BoostScoutRevealRadius += 1;
    }

    public override string GetEffectDescription()
    {
        return "Boosts Scouts'<sprite name=\"Scout_Emoji\"> reveal radius by 1 and they now ignore slow penalty from Mountain, Desert, Swamp and Water tiles";
    }
}

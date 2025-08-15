using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UpgradeScoutLimit")]
public class UpgradeScoutLimit : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.ScoutsLimit++;
    }

    public override string GetEffectDescription()
    {
        return "+1 Scouts limit";
    }
}

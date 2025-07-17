using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UpgradeScoutRevealRadius")]
public class UpgradeScoutRevealRadius : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.BoostScoutRevealRadius++;
    }
}

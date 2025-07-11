using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockScoutDeathReveal")]
public class UnlockScoutDeathReveal : UpgradeEffect
{
    [SerializeField] private int _deathRevealRadius;

    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutRevealOnDeathRadius = _deathRevealRadius;
    }
}

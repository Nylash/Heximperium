using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockScoutDeathReveal")]
public class UnlockScoutDeathReveal : UpgradeEffect
{
    [SerializeField] private int _deathRevealRadius;

    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutRevealOnDeathRadius = _deathRevealRadius;
    }

    public override string GetEffectDescription()
    {
        return $"When a Scout reaches the end of its lifespan, he reveals all tiles in a {_deathRevealRadius}-tiles radius around him";
    }
}

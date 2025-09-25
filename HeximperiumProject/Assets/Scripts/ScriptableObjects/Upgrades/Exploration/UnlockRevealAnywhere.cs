using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Exploration/UnlockRevealAnywhere")]
public class UnlockRevealAnywhere : UpgradeEffect
{
    [SerializeField] private int _revealRadius = 2;

    public int RevealRadius { get => _revealRadius; }

    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeRevealAnywhere = this;
    }
    public override string GetEffectDescription()
    {
        return $"Once per Exploration phase, allow to click on any tile (even an unrevealed one) to reveal it and all those in a {_revealRadius}-tile radius";
    }
}

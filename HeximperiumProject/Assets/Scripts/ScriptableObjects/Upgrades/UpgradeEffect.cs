using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    public string EffectName;
    public Phase AssociatedSystem = Phase.None;

    public abstract void ApplyEffect();

    public abstract string GetEffectDescription();
}

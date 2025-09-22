using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    public string EffectName;

    public abstract void ApplyEffect();

    public abstract string GetEffectDescription();
}

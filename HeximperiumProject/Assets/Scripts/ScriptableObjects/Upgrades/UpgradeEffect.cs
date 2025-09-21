using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    public abstract void ApplyEffect();

    public abstract string GetEffectDescription();
}

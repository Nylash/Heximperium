using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    [SerializeField] private string _effectText;

    public string EffectText { get => _effectText; }

    public abstract void ApplyEffect();
}

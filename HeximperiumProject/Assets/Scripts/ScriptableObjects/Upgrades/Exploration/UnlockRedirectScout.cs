using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockRedirectScout")]
public class UnlockRedirectScout : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutRedirectable = true;
    }

    public override string GetEffectDescription()
    {
        return "Each Scout can be redirected once per turn";
    }
}

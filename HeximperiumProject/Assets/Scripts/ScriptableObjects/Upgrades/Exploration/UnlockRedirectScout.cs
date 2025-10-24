using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Exploration/UnlockRedirectScout")]
public class UnlockRedirectScout : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutRedirectable = true;
    }

    public override string GetEffectDescription()
    {
        return "Each Scout<sprite name=\"Scout_Emoji\"> can be redirected once per turn";
    }
}

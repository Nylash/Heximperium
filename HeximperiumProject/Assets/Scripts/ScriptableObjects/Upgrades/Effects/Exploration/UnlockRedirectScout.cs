using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockRedirectScout")]
public class UnlockRedirectScout : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExplorationManager.Instance.UpgradeScoutRedirectable = true;
    }
}

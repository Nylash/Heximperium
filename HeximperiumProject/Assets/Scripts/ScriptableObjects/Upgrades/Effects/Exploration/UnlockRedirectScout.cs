using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockRedirectScout")]
public class UnlockRedirectScout : UpgradeEffect
{
    public override void ApplyEffect()
    {
        Debug.Log("Unlocking Redirect Scout");
    }
}

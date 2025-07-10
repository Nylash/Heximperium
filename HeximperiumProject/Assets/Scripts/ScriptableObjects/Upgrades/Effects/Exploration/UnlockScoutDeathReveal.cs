using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockScoutDeathReveal")]
public class UnlockScoutDeathReveal : UpgradeEffect
{
    public override void ApplyEffect()
    {
        Debug.Log("Unlocking Scout Death Reveal");
    }
}

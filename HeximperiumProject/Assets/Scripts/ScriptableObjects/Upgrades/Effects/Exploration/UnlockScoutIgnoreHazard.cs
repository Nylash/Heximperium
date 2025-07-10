using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Exploration/UnlockScoutIgnoreHazard")]
public class UnlockScoutIgnoreHazard : UpgradeEffect
{
    public override void ApplyEffect()
    {
        Debug.Log("Unlocking Scout Ignore Hazard");
    }
}

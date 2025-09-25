using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Expansion/UpgradeTownLimit")]
public class UpgradeTownLimit : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExploitationManager.Instance.UpdateTownLimit(3);
    }

    public override string GetEffectDescription()
    {
        return "+3 Towns limit";
    }
}

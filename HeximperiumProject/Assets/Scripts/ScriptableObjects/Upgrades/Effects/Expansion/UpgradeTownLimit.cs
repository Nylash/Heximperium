using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Expansion/UpgradeTownLimit")]
public class UpgradeTownLimit : UpgradeEffect
{
    public override void ApplyEffect()
    {
        ExploitationManager.Instance.UpdateTownLimit(1);
    }

    public override string GetEffectDescription()
    {
        return "+1 Towns limit";
    }
}

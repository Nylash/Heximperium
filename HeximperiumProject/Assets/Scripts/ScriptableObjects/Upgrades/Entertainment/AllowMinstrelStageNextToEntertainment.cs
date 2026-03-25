using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Entertainment/AllowMinstrelStageNextToEntertainment")]
public class AllowMinstrelStageNextToEntertainment : UpgradeEffect
{
    public override void ApplyEffect()
    {
        EntertainmentManager.Instance.UpgradeMinstrelStageOnNeighbor = true;
    }

    public override string GetEffectDescription()
    {
        return "Allows placing <u>Minstrel Stages</u> on tile where a neighbor has an " + Family.Entertainment.ToCustomString();
    }
}

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
        return "Allow placing Minstrel Stages on tile where a neighbor has an entertainment";
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Entertainment/GainCarnivalists")]
public class GainCarnivalists : UpgradeEffect
{
    [SerializeField] private int _carnivalistsToGain = 1;
    [SerializeField] private TileData _emptyDataForSource;

    public override void ApplyEffect()
    {
        ResourcesManager.Instance.UpdateCarnivalist(_carnivalistsToGain, Transaction.Gain);
        ResourcesManager.Instance.UpdateCarnivalistSource(_emptyDataForSource, _carnivalistsToGain, Transaction.Gain);
    }

    public override string GetEffectDescription()
    {
        return $"Recruits {_carnivalistsToGain} <sprite name=\"Carnivalist_Emoji\">";
    }
}

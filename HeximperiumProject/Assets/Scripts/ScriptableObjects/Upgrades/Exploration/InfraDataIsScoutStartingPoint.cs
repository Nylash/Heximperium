using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Exploration/InfraDataIsScoutStartingPoint")]
public class InfraDataIsScoutStartingPoint : UpgradeEffect
{
    [SerializeField] private List<InfrastructureData> _boostedInfra = new List<InfrastructureData>();

    public override void ApplyEffect()
    {
        foreach (InfrastructureData data in _boostedInfra)
        {
            data.ScoutStartingPoint = true;
        }
        if (GameManager.Instance.CurrentPhase == Phase.Explore)
            ExplorationManager.Instance.UpdateInteractableTiles();
    }

    public override string GetEffectDescription()
    {
        return $"Scouts<sprite name=\"Scout_Emoji\"> can start on {_boostedInfra.ToCustomString(true)}";
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/AddSpecialBehaviourToInfraData")]
public class AddSpecialBehaviourToInfraData : UpgradeEffect
{
    [SerializeField] private List<InfrastructureData> _boostedInfra = new List<InfrastructureData>();
    [SerializeField] private SpecialBehaviour _specialBehaviour;

    public override void ApplyEffect()
    {
        foreach (InfrastructureData data in _boostedInfra)
        {
            if (!data.SpecialBehaviours.Contains(_specialBehaviour))
                data.SpecialBehaviours.Add(_specialBehaviour);
        }
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData infraData && _boostedInfra.Contains(infraData))
                _specialBehaviour.InitializeSpecialBehaviour(tile);
        }
    }

    public override string GetEffectDescription()
    {
        return $"{_boostedInfra.ToCustomString(true)} " +
               $"{(_boostedInfra.Count == 1 ? "gains" : "gain")} " +
               $"\"{_specialBehaviour.GetBehaviourDescription()}\"";

    }
}

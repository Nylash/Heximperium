using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private bool isTown;

    private bool _runtimeScoutStartingPoint;

    public bool ScoutStartingPoint { get => _runtimeScoutStartingPoint; set => _runtimeScoutStartingPoint = value; }
    public List<ResourceToIntMap> Costs { get => _costs; }
    public bool IsTown { get => isTown; }

    public override void ResetRuntimeValues()
    {
        base.ResetRuntimeValues();
        _runtimeScoutStartingPoint = _scoutStartingPoint;
    }
}
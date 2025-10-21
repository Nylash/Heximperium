using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private bool isTown;
    [SerializeField][Range(1, 5)] private int _infrastructureLevel;

    private bool _runtimeScoutStartingPoint;

    public bool ScoutStartingPoint { get => _runtimeScoutStartingPoint; set => _runtimeScoutStartingPoint = value; }
    public List<ResourceToIntMap> Costs { get => _costs; }
    public bool IsTown { get => isTown; }
    public int InfrastructureLevel { get => _infrastructureLevel; }

    public override void ResetRuntimeValues()
    {
        base.ResetRuntimeValues();
        _runtimeScoutStartingPoint = _scoutStartingPoint;
    }
}
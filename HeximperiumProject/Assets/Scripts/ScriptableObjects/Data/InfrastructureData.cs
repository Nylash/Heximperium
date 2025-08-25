using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private bool isTown;

    public bool ScoutStartingPoint { get => _scoutStartingPoint; }
    public List<ResourceToIntMap> Costs { get => _costs; }
    public bool IsTown { get => isTown; }
}
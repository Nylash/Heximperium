using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();

    public bool ScoutStartingPoint { get => _scoutStartingPoint; }
    public List<ResourceToIntMap> Costs { get => _costs; }
}
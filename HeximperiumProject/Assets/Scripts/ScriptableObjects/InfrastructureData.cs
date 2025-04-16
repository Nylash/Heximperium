using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private List<ResourceValue> _costs = new List<ResourceValue>();

    public bool ScoutStartingPoint { get => _scoutStartingPoint; }
    public List<ResourceValue> Costs { get => _costs; }
}
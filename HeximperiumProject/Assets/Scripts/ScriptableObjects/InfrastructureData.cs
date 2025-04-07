using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private List<ResourceValue> _costs = new List<ResourceValue>(); 

    public List<ResourceValue> Costs { get => _costs; set => _costs = value; }
}

using System.Collections.Generic;
using UnityEngine;

public class UnitData : ScriptableObject
{
    [SerializeField] private List<ResourceValue> _costs = new List<ResourceValue>();

    public List<ResourceValue> Costs { get => _costs; }
}

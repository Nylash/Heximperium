using System.Collections.Generic;
using UnityEngine;

public class UnitData : ScriptableObject
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();

    public List<ResourceToIntMap> Costs { get => _costs; }
}

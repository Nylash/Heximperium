using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Entertainer")]
public class EntertainerData : UnitData
{
    [SerializeField] private EntertainerType _entertainer;
    [SerializeField] private EntertainerFamily _family;
    [SerializeField] private int _points;
    [SerializeField] private List<EntertainerType> _synergies = new List<EntertainerType>();

    public EntertainerType Entertainer { get => _entertainer; }
    public EntertainerFamily Family { get => _family; }
    public int Points { get => _points; }
    public List<EntertainerType> Synergies { get => _synergies; }
}

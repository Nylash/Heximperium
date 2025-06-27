using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Entertainment")]
public class EntertainmentData : ScriptableObject
{
    [SerializeField] private EntertainmentType _type;
    [SerializeField] private int _basePoints;
    //effect
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();

    public EntertainmentType Type { get => _type; }
    public int BasePoints { get => _basePoints; }
    public List<ResourceToIntMap> Costs { get => _costs; }
}

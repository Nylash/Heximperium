using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scout")]
public class ScoutData : UnitData
{
    [SerializeField] private List<ResourceValue> _costs = new List<ResourceValue>();
    [SerializeField] private int _speed;
    [SerializeField] private int _lifespan;
    [SerializeField] private int _revealRadius;

    public int Speed { get => _speed;}
    public int Lifespan { get => _lifespan;}
    public int RevealRadius { get => _revealRadius;}
    public List<ResourceValue> Costs { get => _costs;}
}

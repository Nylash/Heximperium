using System.Collections.Generic;
using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _claimCost;
    [SerializeField] private string _textEffect;
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private List<ResourceValue> _incomes = new List<ResourceValue>();

    public string TileName { get => _name;}
    public int ClaimCost { get => _claimCost;}
    public string TextEffect { get => _textEffect;}
    public bool ScoutStartingPoint { get => _scoutStartingPoint;}
    public List<ResourceValue> Incomes { get => _incomes;}

    public int GetSpecificIncome(Resource resource)
    {
        foreach (ResourceValue item in _incomes)
        {
            if (item.resource == resource)
                return item.value;
        }
        return 0;
    }
}

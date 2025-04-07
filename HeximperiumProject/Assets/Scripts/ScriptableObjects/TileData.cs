using System.Collections.Generic;
using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _claimCost;
    [SerializeField] private int _goldIncome;
    [SerializeField] private string _textEffect;
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private List<ResourceValue> _incomes = new List<ResourceValue>();

    public string TileName { get => _name;}
    public int ClaimCost { get => _claimCost;}
    public int GoldIncome { get => _goldIncome;}
    public string TextEffect { get => _textEffect;}
    public bool ScoutStartingPoint { get => _scoutStartingPoint;}
    public List<ResourceValue> Incomes { get => _incomes;}
}

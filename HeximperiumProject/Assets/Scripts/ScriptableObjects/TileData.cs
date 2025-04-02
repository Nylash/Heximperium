using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Material _visual;
    [SerializeField] private int _claimCost;
    [SerializeField] private int _goldIncome;

    public Material Visual { get => _visual; set => _visual = value; }
    public string TileName { get => _name; set => _name = value; }
}

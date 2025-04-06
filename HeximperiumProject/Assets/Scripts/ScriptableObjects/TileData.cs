using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _claimCost;
    [SerializeField] private int _goldIncome;
    [SerializeField] private string _textEffect;

    public string TileName { get => _name;}
    public int ClaimCost { get => _claimCost;}
    public int GoldIncome { get => _goldIncome;}
    public string TextEffect { get => _textEffect;}
}

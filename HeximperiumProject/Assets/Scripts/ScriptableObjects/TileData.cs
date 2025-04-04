using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _claimCost;
    [SerializeField] private int _goldIncome;
    [SerializeField] private string _textEffect;

    public string TileName { get => _name; set => _name = value; }
    public int ClaimCost { get => _claimCost; set => _claimCost = value; }
    public int GoldIncome { get => _goldIncome; set => _goldIncome = value; }
    public string TextEffect { get => _textEffect; set => _textEffect = value; }
}

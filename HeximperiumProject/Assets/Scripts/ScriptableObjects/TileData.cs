using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Mesh _visual;
    [SerializeField] private int _claimCost;
    [SerializeField] private int _goldIncome;
}

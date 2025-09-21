using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Entertainment")]
public class EntertainmentData : ScriptableObject
{
    [Header("_________________________________________________________")]
    [Header("Mandatory Settings")]
    [SerializeField] private EntertainmentType _type;
    [SerializeField] private int _basePoints;
    [SerializeField] private int _carnivalistCost;
    [Header("_________________________________________________________")]
    [Header("Optionnal Settings")]
    [SerializeField] private SpecialEffect _specialEffect;

    public EntertainmentType Type { get => _type; }
    public int BasePoints { get => _basePoints; }
    public SpecialEffect SpecialEffect { get => _specialEffect; }

    public int GetActualCarnivalistCost(Tile tile = null)
    {
        int cost = _carnivalistCost;
        if (tile)
            cost -= tile.CarnivalistCostReduction;
        return Mathf.Max(cost, 0);
    }
}

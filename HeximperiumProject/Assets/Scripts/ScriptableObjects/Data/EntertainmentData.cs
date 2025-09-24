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
    [SerializeField] private List<SpecialEffect> _specialEffects;

    public EntertainmentType Type { get => _type; }
    public int BasePoints { get => _basePoints; }
    public List<SpecialEffect> SpecialEffects { get => _specialEffects; }

    public int GetActualCarnivalistCost(Tile tile = null)
    {
        int cost = _carnivalistCost;
        if (tile)
            cost -= tile.CarnivalistCostReduction;
        return Mathf.Max(cost, 0);
    }
}

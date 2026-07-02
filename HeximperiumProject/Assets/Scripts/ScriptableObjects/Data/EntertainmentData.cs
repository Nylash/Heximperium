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
    public List<SpecialEffect> SpecialEffects { get => _runtimeSpecialEffects; }
    public int CarnivalistCost { get => _runtimeCost; set => _runtimeCost = value; }

    private List<SpecialEffect> _runtimeSpecialEffects = new List<SpecialEffect>();
    private int _runtimeCost;

    public int GetActualCarnivalistCost(Tile tile = null)
    {
        int cost = _runtimeCost;
        if (tile)
            cost -= tile.CarnivalistCostReduction;
        return Mathf.Max(cost, 0);
    }

    // Manage runtime list of special effects
    private void OnEnable()
    {
        RuntimeManager.RegisterDataInstance(this);
        ResetRuntimeSpecialEffects();
        _runtimeCost = _carnivalistCost;
    }

    private void OnDisable()
    {
        RuntimeManager.UnregisterDataInstance(this);
    }

    public void ResetRuntimeSpecialEffects()
    {
        _runtimeSpecialEffects = Utilities.CloneScriptableObjects(_specialEffects);
    }
}

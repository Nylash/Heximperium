using System.Collections.Generic;
using UnityEngine;

public class TileData : ScriptableObject
{
    [Header("_________________________________________________________")]
    [Header("Base Settings")]
    [SerializeField] private string _name;
    [Tooltip("Each entry should correspond to a specific base tile then the visual for the current tile on that base tile")]
    [SerializeField] private List<TileDataToGameObjectsMap> _visualsProps;
    [SerializeField] private int _claimCost;
    [SerializeField] private List<ResourceToIntMap> _incomes = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _availableInfrastructures = new List<InfrastructureData>();
    [Header("_________________________________________________________")]
    [Header("Specific Settings")]
    [SerializeField] private List<SpecialBehaviour> _specialBehaviours = new List<SpecialBehaviour>();

    private List<SpecialBehaviour> _runtimeSpecialBehaviours = new List<SpecialBehaviour>();

    public string TileName { get => _name; }
    public virtual int ClaimCost { get => _claimCost; }
    public List<ResourceToIntMap> Incomes { get => _incomes; }
    public List<InfrastructureData> AvailableInfrastructures { get => _availableInfrastructures; }
    public List<SpecialBehaviour> SpecialBehaviours { get => _runtimeSpecialBehaviours; }
    public List<TileDataToGameObjectsMap> VisualsProps { get => _visualsProps; }

    // Manage runtime list of special behaviours
    private void OnEnable()
    {
        RuntimeManager.RegisterDataInstance(this);
        ResetRuntimeSpecialBehaviour();
    }

    private void OnDisable()
    {
        RuntimeManager.UnregisterDataInstance(this);
    }

    public void ResetRuntimeSpecialBehaviour()
    {
        _runtimeSpecialBehaviours = new List<SpecialBehaviour>(_specialBehaviours);
    }
}

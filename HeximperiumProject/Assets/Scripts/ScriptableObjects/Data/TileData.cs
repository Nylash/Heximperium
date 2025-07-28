using System.Collections.Generic;
using UnityEngine;

public class TileData : ScriptableObject
{
    [Header("_________________________________________________________")]
    [Header("Base Settings")]
    [SerializeField] private string _name;
    [SerializeField] private List<Material> _visuals;
    [SerializeField] private int _claimCost;
    [SerializeField] private List<ResourceToIntMap> _incomes = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _availableInfrastructures = new List<InfrastructureData>();
    [Header("_________________________________________________________")]
    [Header("UI Settings")]
    [SerializeField] private string _interactionButtonPopUpText;
    [SerializeField] private string _tilePopUpText;
    [Header("_________________________________________________________")]
    [Header("Specific Settings")]
    [SerializeField] private List<SpecialBehaviour> _specialBehaviours = new List<SpecialBehaviour>();

    public string TileName { get => _name; }
    public virtual int ClaimCost { get => _claimCost; }
    public List<ResourceToIntMap> Incomes { get => _incomes; }
    public List<InfrastructureData> AvailableInfrastructures { get => _availableInfrastructures; }
    public List<SpecialBehaviour> SpecialBehaviours { get => _specialBehaviours; }
    public string InteractionButtonPopUpText { get => _interactionButtonPopUpText; }
    public string TilePopUpText { get => _tilePopUpText; }
    public List<Material> Visuals { get => _visuals; }
}

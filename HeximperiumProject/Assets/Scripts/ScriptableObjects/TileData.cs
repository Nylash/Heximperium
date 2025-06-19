using System.Collections.Generic;
using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _claimCost;
    [SerializeField] private string _interactionButtonPopUpText;
    [SerializeField] private string _tilePopUpText;
    [SerializeField] private TypeIncomeUpgrade _typeIncomeUpgrade;
    [SerializeField] private List<ResourceToIntMap> _incomes = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _availableInfrastructures = new List<InfrastructureData>();
    [SerializeField] private List<SpecialBehaviour> _specialBehaviours = new List<SpecialBehaviour>();
    [SerializeField] private GameObject _popUpPrefab;
    [SerializeField] private List<Material> _visuals;

    public string TileName { get => _name; }
    public int ClaimCost { get => _claimCost; }
    public List<ResourceToIntMap> Incomes { get => _incomes; }
    public TypeIncomeUpgrade TypeIncomeUpgrade { get => _typeIncomeUpgrade; }
    public List<InfrastructureData> AvailableInfrastructures { get => _availableInfrastructures; }
    public List<SpecialBehaviour> SpecialBehaviours { get => _specialBehaviours; }
    public GameObject PopUpPrefab { get => _popUpPrefab; }
    public string InteractionButtonPopUpText { get => _interactionButtonPopUpText; }
    public string TilePopUpText { get => _tilePopUpText; }
    public List<Material> Visuals { get => _visuals; }
}

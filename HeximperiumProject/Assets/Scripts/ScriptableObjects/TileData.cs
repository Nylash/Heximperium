using System.Collections.Generic;
using UnityEngine;

public class TileData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _claimCost;
    [SerializeField] private string _textEffect;
    [SerializeField] private TypeIncomeUpgrade _typeIncomeUpgrade;
    [SerializeField] private List<ResourceValue> _incomes = new List<ResourceValue>();
    [SerializeField] private List<InfrastructureData> _availableInfrastructures = new List<InfrastructureData>();

    public string TileName { get => _name; }
    public int ClaimCost { get => _claimCost; }
    public string TextEffect { get => _textEffect; }
    public List<ResourceValue> Incomes { get => _incomes; }
    public TypeIncomeUpgrade TypeIncomeUpgrade { get => _typeIncomeUpgrade; }
    public List<InfrastructureData> AvailableInfrastructures { get => _availableInfrastructures; }

    public int GetSpecificIncome(Resource resource)
    {
        foreach (ResourceValue item in _incomes)
        {
            if (item.resource == resource)
                return item.value;
        }
        return 0;
    }
}

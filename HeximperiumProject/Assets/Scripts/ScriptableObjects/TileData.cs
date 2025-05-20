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
    [SerializeField] private SpecialBehaviour _specialBehaviour;
    [SerializeField] private GameObject _popUpPrefab;
    [SerializeField] private List<BiomeToMaterialsMap> _biomeMaterials;

    public string TileName { get => _name; }
    public int ClaimCost { get => _claimCost; }
    public List<ResourceToIntMap> Incomes { get => _incomes; }
    public TypeIncomeUpgrade TypeIncomeUpgrade { get => _typeIncomeUpgrade; }
    public List<InfrastructureData> AvailableInfrastructures { get => _availableInfrastructures; }
    public SpecialBehaviour SpecialBehaviour { get => _specialBehaviour; }
    public GameObject PopUpPrefab { get => _popUpPrefab; }
    public string InteractionButtonPopUpText { get => _interactionButtonPopUpText; }
    public string TilePopUpText { get => _tilePopUpText; }

    public int GetSpecificIncome(Resource resource)
    {
        foreach (ResourceToIntMap item in _incomes)
        {
            if (item.resource == resource)
                return item.value;
        }
        return 0;
    }

    public List<Material> GetMaterials(Biome b)
    {
        foreach (BiomeToMaterialsMap item in _biomeMaterials)
        {
            if(item.biome == b)
                return item.materials;
        }
        return new List<Material>();
    }
}

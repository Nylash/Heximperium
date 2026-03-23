using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourcesManager : Singleton<ResourcesManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Trade values")]
    [Header("-BuySR")]
    [SerializeField] private List<ResourceToIntMap> _buySRCost;
    [SerializeField] private List<ResourceToIntMap> _buySRGain;
    [Header("-BuyGold")]
    [SerializeField] private List<ResourceToIntMap> _buyGoldCost;
    [SerializeField] private List<ResourceToIntMap> _buyGoldGain;
    [Header("-BuyClaim")]
    [SerializeField] private List<ResourceToIntMap> _buyClaimCost;
    [SerializeField] private int _buyClaimGain;
    #endregion

    #region VARIABLES
    private int _claim;
    private int _gold;
    private int _specialResources;
    private int _carnivalist;
    //Tracking dictionaries
    private Dictionary<TileData, int> _carnivalistSources = new Dictionary<TileData, int>();
    #endregion

    #region ACCESSORS
    public int Claim { get => _claim; }
    public List<ResourceToIntMap> BuySRCost { get => _buySRCost; }
    public List<ResourceToIntMap> BuySRGain { get => _buySRGain; }
    public List<ResourceToIntMap> BuyGoldCost { get => _buyGoldCost; }
    public List<ResourceToIntMap> BuyGoldGain { get => _buyGoldGain; }
    public List<ResourceToIntMap> BuyClaimCost { get => _buyClaimCost; }
    public int BuyClaimGain { get => _buyClaimGain; }
    public int Carnivalist { get => _carnivalist; }
    public Dictionary<TileData, int> CarnivalistSources { get => _carnivalistSources; }

    public int GetResourceStock(Resource resource)
    {
        switch (resource)
        {
            case Resource.Gold:
                return _gold;
            case Resource.SpecialResources:
                return _specialResources;
            default:
                return 0;
        }
    }
    #endregion

    #region EVENTS
    public event Action<Tile, int> OnGoldGained;
    public event Action<Tile, int> OnSpecialResourcesGained;
    public event Action<Tile, int> OnClaimGained;
    public event Action<Tile, int> OnCarnivalistGained;
    public event Action<int> OnGoldSpent;
    public event Action<int> OnSpecialResourcesSpent;
    public event Action<int> OnClaimSpent;
    public event Action<Tile, int> OnCarnivalistSpent;
    #endregion

    public void CHEAT_RESOURCES()
    {
        Debug.LogWarning("USING CHEAT !");
        UpdateResource(Resource.Gold, 5000, Transaction.Gain);
        UpdateResource(Resource.SpecialResources, 1000, Transaction.Gain);
    }

    #region UPDATE RESOURCES
    private void UpdateResource(Resource resource, int value, Transaction transaction)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        switch (resource)
        {
            case Resource.Gold:
                _gold += value;
                if (_gold < 0)
                    _gold = 0;
                UIManager.Instance.UpdateResourceUI(Resource.Gold, _gold);
                break;
            case Resource.SpecialResources:
                _specialResources += value;
                if (_specialResources < 0)
                    _specialResources = 0;
                UIManager.Instance.UpdateResourceUI(Resource.SpecialResources, _specialResources);
                break;
        }
    }

    public void UpdateResource(List<ResourceToIntMap> resources, Transaction transaction, Tile tile = null)
    {
        if (resources.Count == 0) return;
        foreach (ResourceToIntMap item in resources)
        {
            UpdateResource(item.resource, item.value, transaction);

            switch (transaction)
            {
                case Transaction.Gain:
                    switch (item.resource)
                    {
                        case Resource.Gold:
                            OnGoldGained?.Invoke(tile, item.value);
                            break;
                        case Resource.SpecialResources:
                            OnSpecialResourcesGained?.Invoke(tile, item.value);
                            break;
                    }
                    break;
                case Transaction.Spent:
                    switch (item.resource)
                    {
                        case Resource.Gold:
                            OnGoldSpent?.Invoke(item.value);
                            break;
                        case Resource.SpecialResources:
                            OnSpecialResourcesSpent?.Invoke(item.value);
                            break;
                    }
                    break;
            }
        }
    }

    public void UpdateClaim(int value, Transaction transaction, Tile tile = null)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        _claim += value;
        if(_claim < 0)
            _claim = 0;
        UIManager.Instance.UpdateClaimUI(_claim);

        switch (transaction)
        {
            case Transaction.Gain:
                OnClaimGained?.Invoke(tile, value);
                break;
            case Transaction.Spent:
                OnClaimSpent?.Invoke(Mathf.Abs(value));
                break;
        }
    }

    public void UpdateCarnivalist(int value, Transaction transaction, Tile tile = null)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        _carnivalist += value;
        if (_carnivalist < 0)
            _carnivalist = 0;
        UIManager.Instance.UpdateCarnivalistUI(_carnivalist);

        if (!tile)
        {
            switch (transaction)
            {
                case Transaction.Gain:
                    HelperOnCarnivalistGained(null, value);
                    break;
                case Transaction.Spent:
                    HelperOnCarnivalistSpent(null, value);
                    break;
            }
        }
    }

    public void HelperOnCarnivalistGained(Tile tile, int value)
    {
        if (value == 0)
            return;
        OnCarnivalistGained?.Invoke(tile, value);
    }

    public void HelperOnCarnivalistSpent(Tile tile, int value)
    {
        if (value == 0)
            return;
        OnCarnivalistSpent?.Invoke(tile, Mathf.Abs(value));
    }

    public void UpdateCarnivalistSource(TileData source, int value, Transaction transaction)
    {
        switch (transaction)
        {
            case Transaction.Gain:
                if (!_carnivalistSources.ContainsKey(source))
                    _carnivalistSources.Add(source, 0);
                _carnivalistSources[source] += value;
                break;
            case Transaction.Spent:
                if (!_carnivalistSources.ContainsKey(source))
                    Debug.LogError("Trying to spend carnivalist from a source that doesn't exist in the dictionary");
                _carnivalistSources[source] -= value;
                if (_carnivalistSources[source] <= 0)
                    _carnivalistSources.Remove(source);
                break;
            default:
                break;
        }
    }

    public void SpendConvertedResources(int goldConverted, int srConverted)
    {
        UpdateResource(Resource.Gold, goldConverted, Transaction.Spent);
        UpdateResource(Resource.SpecialResources, srConverted, Transaction.Spent);
    }
    #endregion

    #region CHECK RESOURCES
    public bool CanAffordClaim(int claim)
    {
        if (_claim - claim >= 0)
            return true;
        else
            return false;
    }

    public bool CanAffordCarnivalist(int carnivalist)
    {
        if (_carnivalist - carnivalist >= 0)
            return true;
        else
            return false;
    }

    private bool CanAfford(Resource resource, int cost)
    {
        switch (resource)
        {
            case Resource.Gold:
                if (_gold - cost >= 0)
                    return true;
                else
                    return false;
            case Resource.SpecialResources:
                if (_specialResources - cost >= 0)
                    return true;
                else
                    return false;
        }
        return false;
    }

    public bool CanAfford(List<ResourceToIntMap> costs) 
    {
        if(costs.Count == 0) return true;
        return costs.All(cost => CanAfford(cost.resource, cost.value));
    }
    #endregion
}
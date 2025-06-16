using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourcesManager : Singleton<ResourcesManager>
{
    #region CONFIGURATION
    [SerializeField] private GameObject _resourceGainPrefab;
    [SerializeField] private Material _goldVFXMat;
    [SerializeField] private Material _srVFXMat;
    #endregion

    #region VARIABLES
    private int _claim;
    private int _gold;
    private int _specialResources;
    private int _srReductionForExploration;
    private int _srReductionForExpansion;
    private int _srReductionForExploitation;
    private int _srReductionForEntertainment;
    #endregion

    #region ACCESSORS
    public int Claim { get => _claim; }

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

    public int GetSRReduction(Phase system)
    {
        switch (system)
        {
            case Phase.Explore:
                return _srReductionForExploration;
            case Phase.Expand:
                return _srReductionForExpansion;
            case Phase.Exploit:
                return _srReductionForExploitation;
            case Phase.Entertain:
                return _srReductionForEntertainment;
            default:
                return 0;
        }
    }

    public void SetSSRReduction(Phase system, int value)
    {
        switch (system)
        {
            case Phase.Explore:
                _srReductionForExploration += value;
                break;
            case Phase.Expand:
                _srReductionForExpansion += value;
                break;
            case Phase.Exploit:
                _srReductionForExploitation += value;
                break;
            case Phase.Entertain:
                _srReductionForEntertainment += value;
                break;
            default:
                break;
        }
    }
    #endregion

    public void CHEAT_GAIN_ALL_RESOURCES()
    {
        Debug.LogWarning("USING CHEAT !");
        UpdateResource(Resource.Gold, 500, Transaction.Gain);
        UpdateResource(Resource.SpecialResources, 100, Transaction.Gain);
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
                UIManager.Instance.UpdateResourceUI(Resource.Gold, _gold);
                break;
            case Resource.SpecialResources:
                _specialResources += value;
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

            //Play VFX if we gain resource from a tile
            if( tile != null && transaction == Transaction.Gain)
            {
                switch (item.resource)
                {
                    case Resource.Gold:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _goldVFXMat, item.value);
                        break;
                    case Resource.SpecialResources:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _srVFXMat, item.value);
                        break;
                }
            }
        }
    }

    public void UpdateClaim(int value, Transaction transaction)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        _claim += value;
        UIManager.Instance.UpdateClaimUI(_claim);
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

    private bool CanAfford(Resource resource, int cost)
    {
        switch (resource)
        {
            case Resource.Gold:
                return CanAffordUnspecified(_gold, cost);
            case Resource.SpecialResources:
                return CanAffordUnspecified(_specialResources, cost);
        }
        return false;
    }

    public bool CanAfford(List<ResourceToIntMap> costs) 
    {
        if(costs.Count == 0) return true;
        return costs.All(cost => CanAfford(cost.resource, cost.value));
    }

    private bool CanAffordUnspecified(int stock, int cost)
    {
        if (stock - cost >= 0)
            return true;
        else
            return false;
    }
    #endregion
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourcesManager : Singleton<ResourcesManager>
{
    #region CONFIGURATION
    [SerializeField] private GameObject _resourceGainPrefab;
    [SerializeField] private Material _goldVFXMat;
    [SerializeField] private Material _stoneVFXMat;
    [SerializeField] private Material _essenceVFXMat;
    [SerializeField] private Material _horseVFXMat;
    [SerializeField] private Material _pigmentVFXMat;
    [SerializeField] private Material _crystalVFXMat;
    [SerializeField] private Material _emberboneVFXMat;
    #endregion

    #region VARIABLES
    private int _claim;
    private int _gold;
    private int _stone;
    private int _essence;
    private int _horse;
    private int _pigment;
    private int _crystal;
    private int _emberbone;
    #endregion

    #region ACCESSORS
    public int Claim { get => _claim; }
    #endregion

    public int GetResourceStock(Resource resource)
    {
        switch (resource)
        {
            case Resource.Stone:
                return _stone;
            case Resource.Essence:
                return _essence;
            case Resource.Horse:
                return _horse;
            case Resource.Pigment:
                return _pigment;
            case Resource.Crystal:
                return _crystal;
            case Resource.Emberbone:
                return _emberbone;
            case Resource.Gold:
                return _gold;
            default:
                return 0;
        }
    }

    public void CHEAT_GAIN_ALL_RESOURCES()
    {
        Debug.LogWarning("USING CHEAT !");
        UpdateResource(Resource.Stone, 50, Transaction.Gain);
        UpdateResource(Resource.Essence, 50, Transaction.Gain);
        UpdateResource(Resource.Horse, 50, Transaction.Gain);
        UpdateResource(Resource.Pigment, 50, Transaction.Gain);
        UpdateResource(Resource.Crystal, 50, Transaction.Gain);
        UpdateResource(Resource.Emberbone, 50, Transaction.Gain);
        UpdateResource(Resource.Gold, 500, Transaction.Gain);
    }

    #region UPDATE RESOURCES
    private void UpdateResource(Resource resource, int value, Transaction transaction)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        switch (resource)
        {
            case Resource.Stone:
                _stone += value;
                UIManager.Instance.UpdateResourceUI(Resource.Stone, _stone);
                break;
            case Resource.Essence:
                _essence += value;
                UIManager.Instance.UpdateResourceUI(Resource.Essence, _essence);
                break;
            case Resource.Horse:
                _horse += value;
                UIManager.Instance.UpdateResourceUI(Resource.Horse, _horse);
                break;
            case Resource.Pigment:
                _pigment += value;
                UIManager.Instance.UpdateResourceUI(Resource.Pigment, _pigment);
                break;
            case Resource.Crystal:
                _crystal += value;
                UIManager.Instance.UpdateResourceUI(Resource.Crystal, _crystal);
                break;
            case Resource.Emberbone:
                _emberbone += value;
                UIManager.Instance.UpdateResourceUI(Resource.Emberbone, _emberbone);
                break;
            case Resource.Gold:
                _gold += value;
                UIManager.Instance.UpdateResourceUI(Resource.Gold, _gold);
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
                    case Resource.Stone:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _stoneVFXMat, item.value);
                        break;
                    case Resource.Essence:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _essenceVFXMat, item.value);
                        break;
                    case Resource.Horse:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _horseVFXMat, item.value);
                        break;
                    case Resource.Pigment:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _pigmentVFXMat, item.value);
                        break;
                    case Resource.Crystal:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _crystalVFXMat, item.value);
                        break;
                    case Resource.Emberbone:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _emberboneVFXMat, item.value);
                        break;
                    case Resource.Gold:
                        Utilities.PlayResourceGainVFX(tile, _resourceGainPrefab, _goldVFXMat, item.value);
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
            case Resource.Stone:
                return CanAffordUnspecified(_stone, cost);
            case Resource.Essence:
                return CanAffordUnspecified(_essence, cost);
            case Resource.Horse:
                return CanAffordUnspecified(_horse, cost);
            case Resource.Pigment:
                return CanAffordUnspecified(_pigment, cost);
            case Resource.Crystal:
                return CanAffordUnspecified(_crystal, cost);
            case Resource.Emberbone:
                return CanAffordUnspecified(_emberbone, cost);
            case Resource.Gold:
                return CanAffordUnspecified(_gold, cost);
        }
        return false;
    }

    public bool CanAfford(List<ResourceToIntMap> costs) 
    {
        if(costs.Count == 0) return false;
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
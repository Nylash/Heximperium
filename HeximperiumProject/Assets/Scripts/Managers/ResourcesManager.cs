using System.Collections.Generic;
using System.Linq;

public class ResourcesManager : Singleton<ResourcesManager>
{
    private int _claim;
    private int _gold;
    private int _stone;
    private int _essence;
    private int _horse;
    private int _pigment;
    private int _crystal;
    private int _emberbone;

    public void UpdateResource(Resource resource, int value, Transaction transaction)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        switch (resource)
        {
            case Resource.Stone:
                break;
            case Resource.Essence:
                break;
            case Resource.Horse:
                break;
            case Resource.Pigment:
                break;
            case Resource.Crystal:
                break;
            case Resource.Emberbone:
                break;
            case Resource.Gold:
                _gold += value;
                UIManager.Instance.UpdateResourceUI(Resource.Gold, _gold);
                break;
        }
    }

    public void UpdateClaim(int value, Transaction transaction)
    {
        if (transaction == Transaction.Spent)
            value = -value;
        _claim += value;
        UIManager.Instance.UpdateClaimUI(_claim);
    }

    public void UpdateResource(List<ResourceValue> resources, Transaction transaction)
    {
        if (resources.Count == 0) return;
        foreach (ResourceValue item in resources)
        {
            UpdateResource(item.resource, item.value, transaction);
        }
    }

    public bool CanAffordClaim(int claim)
    {
        if (_claim - claim >= 0)
            return true;
        else
            return false;
    }

    public bool CanAfford(Resource resource, int cost)
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

    public bool CanAfford(List<ResourceValue> costs) 
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
}


public enum Resource
{
    Stone, Essence, Horse, Pigment, Crystal, Emberbone, Gold
}

public enum Transaction
{
    Gain, Spent
}
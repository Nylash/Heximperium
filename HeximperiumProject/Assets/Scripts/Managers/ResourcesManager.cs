using UnityEngine;
using UnityEngine.Events;

public class ResourcesManager : Singleton<ResourcesManager>
{
    private int _claim;

    public int Claim { 
        get => _claim; 
        set 
        {
            _claim = value;
            UIManager.Instance.UpdateResourceUI(Resource.Claim, _claim);
        } 
    }

    public bool CanAfford(Resource resource, int cost)
    {
        switch (resource)
        {
            case Resource.Stone:
                return false;
            case Resource.Essence:
                return false;
            case Resource.Horse:
                return false;
            case Resource.Pigment:
                return false;
            case Resource.Crystal:
                return false;
            case Resource.Emberbone:
                return false;
            case Resource.Claim:
                if(_claim - cost >= 0)
                    return true;
                else
                    return false;
            case Resource.Point:
                return false;
        }
        return false;
    }
}

public enum Resource
{
    Stone, Essence, Horse, Pigment, Crystal, Emberbone, Claim, Point
}
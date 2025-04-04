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
}

public enum Resource
{
    Stone, Essence, Horse, Pigment, Crystal, Emberbone, Claim, Point
}
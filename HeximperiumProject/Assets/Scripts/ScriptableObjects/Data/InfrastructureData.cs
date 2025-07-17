using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [SerializeField] private Phase _associatedSystem = Phase.None;
    [SerializeField] private bool _scoutStartingPoint;
    [SerializeField] private bool isTown;

    public bool ScoutStartingPoint { get => _scoutStartingPoint; }
    public List<ResourceToIntMap> Costs 
    { 
        get
        {
            if(_associatedSystem == Phase.None)
                return _costs;
            List<ResourceToIntMap> reductedCost = Utilities.CloneResourceToIntMap(_costs);
            foreach (ResourceToIntMap item in reductedCost)
            {
                if (item.resource == Resource.SpecialResources)
                    item.value -= ResourcesManager.Instance.GetSRReduction(_associatedSystem);
                if (item.value < 0)
                    item.value = 0;
            }
            return reductedCost;
        }   
    }
    public bool IsTown { get => isTown; }
}
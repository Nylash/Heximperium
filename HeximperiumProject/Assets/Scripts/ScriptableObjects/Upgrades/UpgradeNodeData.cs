using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Node")]
public class UpgradeNodeData : ScriptableObject
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [SerializeField] private UpgradeEffect _effect;
    [Tooltip("Only one of these are required to unlock this node.")]
    [SerializeField] private List<UpgradeNodeData> _prerequisites;
    [SerializeField] private UpgradeNodeData _exclusiveNode;
    [SerializeField] private Phase _associatedSystem = Phase.None;

    public List<ResourceToIntMap> Costs
    {
        get
        {
            if (_associatedSystem == Phase.None)
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
    public UpgradeEffect Effect { get => _effect; }
    public List<UpgradeNodeData> Prerequisites { get => _prerequisites; }
    public UpgradeNodeData ExclusiveNode { get => _exclusiveNode; }
}

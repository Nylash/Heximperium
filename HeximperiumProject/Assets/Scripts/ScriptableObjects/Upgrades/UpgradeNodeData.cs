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

    public List<ResourceToIntMap> Costs { get => _costs; }
    public UpgradeEffect Effect { get => _effect; }
    public List<UpgradeNodeData> Prerequisites { get => _prerequisites; }
    public UpgradeNodeData ExclusiveNode { get => _exclusiveNode; }
}

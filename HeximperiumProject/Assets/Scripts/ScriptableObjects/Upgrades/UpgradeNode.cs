using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades Tree/Node")]
public class UpgradeNode : ScriptableObject
{
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [SerializeField] private UpgradeEffect _effect;
    [Tooltip("Only one of these are required to unlock this node.")]
    [SerializeField] private List<UpgradeNode> _prerequisites;
    [SerializeField] private UpgradeNode _exclusiveNode;

    public List<ResourceToIntMap> Costs { get => _costs; }
    public UpgradeEffect Effect { get => _effect; }
    public List<UpgradeNode> Prerequisites { get => _prerequisites; }
    public UpgradeNode ExclusiveNode { get => _exclusiveNode; }
}

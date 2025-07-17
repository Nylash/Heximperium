using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradesManager : Singleton<UpgradesManager>
{
    private List<UpgradeNodeData> _unlockedNodes = new List<UpgradeNodeData>();

    public void UnlockNode(UpgradeNodeData node)
    {
        if (CanUnlockNode(node) == UpgradeStatus.Unlockable)
        {
            ResourcesManager.Instance.UpdateResource(node.Costs, Transaction.Spent);
            _unlockedNodes.Add(node);
            node.Effect.ApplyEffect();

            foreach (UI_UpgradeNode n in UIManager.Instance.ActivatedTree.nodes)
                n.UpdateVisual();
        }
    }

    public UpgradeStatus CanUnlockNode(UpgradeNodeData node)
    {
        if (_unlockedNodes.Contains(node))
            return UpgradeStatus.Unlocked;

        if (node.Prerequisites.Count > 0 && !_unlockedNodes.Any(item => node.Prerequisites.Contains(item)))
            return UpgradeStatus.LockedByPrerequisites;

        if (_unlockedNodes.Contains(node.ExclusiveNode))
            return UpgradeStatus.LockedByExclusive;

        if (ResourcesManager.Instance.CanAfford(node.Costs))
            return UpgradeStatus.Unlockable;
        else
            return UpgradeStatus.CantAfford;
    }
}

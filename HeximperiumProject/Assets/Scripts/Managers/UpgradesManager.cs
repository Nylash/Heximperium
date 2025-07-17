using System;
using System.Collections.Generic;
using System.Linq;

public class UpgradesManager : Singleton<UpgradesManager>
{
    private List<UpgradeNodeData> _unlockedNodes = new List<UpgradeNodeData>();

    public Action<UI_UpgradeNode> OnNodeUnlocked;

    public void UnlockNode(UI_UpgradeNode node)
    {
        if (CanUnlockNode(node.NodeData) == UpgradeStatus.Unlockable)
        {
            ResourcesManager.Instance.UpdateResource(node.NodeData.Costs, Transaction.Spent);
            _unlockedNodes.Add(node.NodeData);
            node.NodeData.Effect.ApplyEffect();

            foreach (UI_UpgradeNode n in UIManager.Instance.ActivatedTree.nodes)
                n.UpdateVisual();

            OnNodeUnlocked?.Invoke(node);
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

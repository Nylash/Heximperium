using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class UpgradesManager : Singleton<UpgradesManager>
{
    private List<UpgradeNodeData> _unlockedNodes = new List<UpgradeNodeData>();

    public void UnlockNode(UpgradeNodeData node)
    {
        if (CanUnlockNode(node))
        {
            ResourcesManager.Instance.UpdateResource(node.Costs, Transaction.Spent);
            _unlockedNodes.Add(node);
            node.Effect.ApplyEffect();
        }
    }

    public bool CanUnlockNode(UpgradeNodeData node)
    {
        if (_unlockedNodes.Contains(node))
        {
            print($"Node {node.name} is already unlocked.");
            return false;
        }

        if (node.Prerequisites.Count > 0 && !_unlockedNodes.Any(item => node.Prerequisites.Contains(item)))
        {
            print($"Node {node.name} cannot be unlocked because its prerequisites are not met.");
            return false;
        }

        if (_unlockedNodes.Contains(node.ExclusiveNode))
        {
            print($"Node {node.name} cannot be unlocked because its exclusive node {_unlockedNodes.First(item => item == node.ExclusiveNode).name} is already unlocked.");
            return false;
        }
        if (ResourcesManager.Instance.CanAfford(node.Costs))
        {
            return true;
        }
        else
        {
            print($"Node {node.name} cannot be unlocked because the player does not have enough resources.");
            return false;
        }
    }
}

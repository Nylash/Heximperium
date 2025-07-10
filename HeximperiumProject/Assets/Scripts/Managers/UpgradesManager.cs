using System.Collections.Generic;
using System.Linq;

public class UpgradesManager : Singleton<UpgradesManager>
{
    private List<UpgradeNode> _unlockedNodes = new List<UpgradeNode>();

    public void UnlockNode(UpgradeNode node)
    {
        if (_unlockedNodes.Contains(node))
        {
            print($"Node {node.name} is already unlocked.");
            return;
        }
            
        if (node.Prerequisites.Count > 0 && !_unlockedNodes.Any(item => node.Prerequisites.Contains(item)))
        {
            print($"Node {node.name} cannot be unlocked because its prerequisites are not met.");
            return;
        }
            
        if (_unlockedNodes.Contains(node.ExclusiveNode))
        {
            print($"Node {node.name} cannot be unlocked because its exclusive node {_unlockedNodes.First(item => item == node.ExclusiveNode).name} is already unlocked.");
            return;
        }
        if (ResourcesManager.Instance.CanAfford(node.Costs))
        {
            ResourcesManager.Instance.UpdateResource(node.Costs, Transaction.Spent);
            _unlockedNodes.Add(node);
            node.Effect.ApplyEffect();
        }
        else
        {
            print($"Node {node.name} cannot be unlocked because the player does not have enough resources.");
        }
    }   
}

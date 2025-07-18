using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeWhenScoutRevealTile")]
public class IncomeWhenScoutRevealTile : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _income = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.OnScoutSpawned -= behaviourTile.ListenerOnScoutSpawned_GainIncomeWhenScoutRevealTile;
        ExplorationManager.Instance.OnScoutSpawned += behaviourTile.ListenerOnScoutSpawned_GainIncomeWhenScoutRevealTile;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.OnScoutSpawned -= behaviourTile.ListenerOnScoutSpawned_GainIncomeWhenScoutRevealTile;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }

    public void CheckScoutSpawned(Tile behaviourTile, Scout scout)
    {
        if (scout.CurrentTile == behaviourTile)
        {
            scout.OnScoutRevealingTile += behaviourTile.ListenerOnScoutRevealingTile;
        }
    }

    public void TileRevealed(Tile behaviourTile)
    {
        ResourcesManager.Instance.UpdateResource(_income, Transaction.Gain, behaviourTile);
    }
}

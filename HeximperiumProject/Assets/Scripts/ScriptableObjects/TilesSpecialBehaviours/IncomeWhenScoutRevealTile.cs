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

        foreach (Scout scout in ExplorationManager.Instance.Scouts)
        {
            CheckScoutSpawned(behaviourTile, scout);
        }
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
        scout.OnScoutRevealingTile += behaviourTile.ListenerOnScoutRevealingTile;
    }

    public void TileRevealed(Tile behaviourTile)
    {
        ResourcesManager.Instance.UpdateResource(_income, Transaction.Gain, behaviourTile);
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain {_income.IncomeToString()} when a scout reveals a tile";
    }
}

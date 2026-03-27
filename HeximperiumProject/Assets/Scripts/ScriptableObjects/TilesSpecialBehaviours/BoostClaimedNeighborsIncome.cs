using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostClaimedNeighborsIncome")]
public class BoostClaimedNeighborsIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;
            if (neighbor.Claimed)
            {
                ApplyBoostToClaimedTile(behaviourTile, neighbor);
            }
            else
            {
                //If the neighbor isn't claimed add a listener to boost it when it will be claimed
                //BehaviourTile is needed even if the reference isn't in the method to create a unique pair of behaviourTile and neighbor, avoiding conflict between events
                neighbor.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome;
                neighbor.OnTileClaimed += behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome;
            }
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;
            if (neighbor.Claimed)
            {
                neighbor.UpdateIncomes(_incomeBoost, false, behaviourTile);
            }
            else
            {
                neighbor.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome;
            }
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;
            if (neighbor.Claimed)
            {
                neighbor.Highlight(show);
            }
        }
    }

    public void ApplyBoostToClaimedTile(Tile behaviourTile, Tile boostedTile)
    {
        boostedTile.UpdateIncomes(_incomeBoost, true, behaviourTile);
        boostedTile.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome;
    }

    public override string GetBehaviourDescription()
    {
        return $"Boosts the income of claimed neighbors by {_incomeBoost.IncomeToString()}";
    }
}

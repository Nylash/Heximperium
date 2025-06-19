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
            if (neighbor.Claimed)
            {
                ApplyBoostToClaimedTile(behaviourTile, neighbor);
            }
            else
            {
                //If the neighbor isn't claimed add a listener to boost it when it will be claimed
                //BehaviourTile is needed even if the reference isn't in the method to create a unique pair of behaviourTile and neighbor, avoiding conflict between events
                neighbor.OnTileClaimed.RemoveListener(behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome);
                neighbor.OnTileClaimed.AddListener(behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome);
            }
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                neighbor.Incomes = Utilities.SubtractResourceToIntMaps(neighbor.Incomes, _incomeBoost);
            }
            else
            {
                neighbor.OnTileClaimed.RemoveListener(behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome);
            }
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                neighbor.Highlight(show);
            }
        }
    }

    public void ApplyBoostToClaimedTile(Tile behaviourTile, Tile boostedTile)
    {
        boostedTile.Incomes = Utilities.MergeResourceToIntMaps(boostedTile.Incomes, _incomeBoost);
        boostedTile.OnTileClaimed.RemoveListener(behaviourTile.ListenerOnTileClaimed_BoostClaimedNeighborsIncome);
    }
}

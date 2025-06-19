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
                ApplyBoost(neighbor);
            }
            else
            {
                //If the neighbor isn't claimed add a listener to boost it when it will be claimed
                neighbor.OnTileClaimed.RemoveListener(ApplyBoost);
                neighbor.OnTileClaimed.AddListener(ApplyBoost);
            }
        }
    }

    public override void InitializeSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Nothing needed, the basic initialization of the behaviour is enough
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
                neighbor.OnTileClaimed.RemoveListener(ApplyBoost);
            }
        }
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Nothing needed, the change on another tile can't undo this behaviour (a tile can't be unclaimed)
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

    private void ApplyBoost(Tile boostedTile)
    {
        boostedTile.Incomes = Utilities.MergeResourceToIntMaps(boostedTile.Incomes, _incomeBoost);
        boostedTile.OnTileClaimed.RemoveListener(ApplyBoost);
    }
}

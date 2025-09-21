using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostAllNeighborsIncome")]
public class BoostAllNeighborsIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.Incomes = Utilities.MergeResourceToIntMaps(neighbor.Incomes, _incomeBoost);
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.Incomes = Utilities.SubtractResourceToIntMaps(neighbor.Incomes, _incomeBoost);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.Highlight(show);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Boosts the income of every neighbors by {_incomeBoost.IncomeToString()}";
    }
}

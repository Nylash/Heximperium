using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeCoomingFromneighbors")]
public class IncomeComingFromNeighbors : SpecialBehaviour
{
    private void AddListener(Tile tile)
    {
        tile.event_IncomeModified.AddListener(AdjustIncomeFromNeighbor);
    }

    public override void RealizeSpecialBehaviour()
    {
        foreach (Tile neighbor in _tile.Neighbors)
        {
            AddListener(neighbor);
            if (neighbor.Claimed)
                _tile.Incomes = Utilities.MergeResourceValues(_tile.Incomes, neighbor.Incomes);
        }
    }

    public override void RealizeSpecialBehaviour(Tile specificTile)
    {
        //Handle by the event
    }

    public override void RollbackSpecialBehaviour()
    {
        //Nothing needed, replacing by previous tile will be enough (this behaviour only modify its own tile)
    }

    private void AdjustIncomeFromNeighbor(List<ResourceValue> previousIncome, List<ResourceValue> newIncome)
    {
        foreach (ResourceValue item in previousIncome)
        {
            item.value = -item.value;
        }
        //Remove previous income
        _tile.Incomes = Utilities.MergeResourceValues(_tile.Incomes, previousIncome);
        //Add new income
        _tile.Incomes = Utilities.MergeResourceValues(_tile.Incomes, newIncome);
    }
}

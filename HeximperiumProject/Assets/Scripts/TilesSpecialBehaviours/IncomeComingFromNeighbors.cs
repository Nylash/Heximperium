using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeCoomingFromneighbors")]
public class IncomeComingFromNeighbors : SpecialBehaviour
{
    //When a neighbor has its income modify we need to mofidy this one accordingly
    private void AddListener(Tile tile)
    {
        tile.OnIncomeModified.AddListener(AdjustIncomeFromNeighbor);
    }

    //Subscribe to neighbors event OnIncomeModified and update its income based on their income
    public override void InitializeSpecialBehaviour()
    {
        foreach (Tile neighbor in _tile.Neighbors)
        {
            AddListener(neighbor);
            if (neighbor.Claimed)
                _tile.Incomes = Utilities.MergeResourceValues(_tile.Incomes, neighbor.Incomes);
        }
    }

    public override void ApplySpecialBehaviour(Tile specificTile)
    {
        //Nothing needed, this behaviour doesn't impact others tiles
    }

    public override void RollbackSpecialBehaviour()
    {
        //Nothing needed, replacing by previous tile will be enough (this behaviour only modify its own tile)
    }

    private void AdjustIncomeFromNeighbor(List<ResourceValue> previousIncome, List<ResourceValue> newIncome)
    {
        //Switch the previous income to negative value
        foreach (ResourceValue item in previousIncome)
            item.value = -item.value;
        //Remove previous income
        _tile.Incomes = Utilities.MergeResourceValues(_tile.Incomes, previousIncome);
        //Add new income
        _tile.Incomes = Utilities.MergeResourceValues(_tile.Incomes, newIncome);
    }
}

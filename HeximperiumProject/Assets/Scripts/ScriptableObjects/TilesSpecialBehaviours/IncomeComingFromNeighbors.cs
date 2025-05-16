using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeCoomingFromneighbors")]
public class IncomeComingFromNeighbors : SpecialBehaviour
{
    [SerializeField] private Resource _resource;

    //Subscribe to neighbors event OnIncomeModified and update its income based on their income
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                List<ResourceValue> income = new List<ResourceValue>();

                foreach (ResourceValue item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        income.Add(new ResourceValue(_resource, item.value));
                }

                behaviourTile.Incomes = Utilities.MergeResourceValues(behaviourTile.Incomes, income);
            }
        }
    }

    public override void ApplySpecialBehaviour(Tile specificTile)
    {
        //Nothing needed, this behaviour doesn't impact others tiles
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Nothing needed, replacing by previous tile will be enough (this behaviour only modify its own tile)
    }

    public void AdjustIncomeFromNeighbor(Tile behaviourTile, List<ResourceValue> previousIncome, List<ResourceValue> newIncome)
    {
        //Switch the previous income to negative value
        foreach (ResourceValue item in previousIncome)
            item.value = -item.value;
        //Remove previous income
        behaviourTile.Incomes = Utilities.MergeResourceValues(behaviourTile.Incomes, previousIncome);
        //Add new income
        behaviourTile.Incomes = Utilities.MergeResourceValues(behaviourTile.Incomes, newIncome);
    }

    public void AddClaimedTileIncome(Tile behaviourTile, Tile tile)
    {
        behaviourTile.Incomes = Utilities.MergeResourceValues(behaviourTile.Incomes, tile.Incomes);
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                foreach (ResourceValue item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        neighbor.BoostHighlight(show);
                }
            }
        }
    }
}

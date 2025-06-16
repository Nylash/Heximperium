using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeCoomingFromneighbors")]
public class IncomeComingFromNeighbors : SpecialBehaviour
{
    [SerializeField] private Resource _resource;
    [SerializeField] private List<InfrastructureData> _excludedInfra = new List<InfrastructureData>();

    //Subscribe to neighbors event OnIncomeModified and update its income based on their income
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                //Don't do the adjustement if the neighbor is excluded
                bool exclude = false;
                if (neighbor.TileData is InfrastructureData)
                {
                    foreach (InfrastructureData item in _excludedInfra)
                    {
                        if (item == neighbor.TileData)
                        {
                            exclude = true;
                            break;
                        }
                    }
                }
                if (exclude)
                    continue;

                List<ResourceToIntMap> income = new List<ResourceToIntMap>();

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        income.Add(new ResourceToIntMap(_resource, item.value));
                }

                behaviourTile.Incomes = Utilities.MergeResourceValues(behaviourTile.Incomes, income);
            }
        }
    }

    public override void ApplySpecialBehaviourToSpecificTile(Tile specificTile)
    {
        //Nothing needed, this behaviour doesn't impact others tiles
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Nothing needed, replacing by previous tile will be enough (this behaviour only modify its own tile)
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile)
    {
        //Nothing needed everything is handled by the event
    }

    public void AdjustIncomeFromNeighbor(Tile neighbor, Tile behaviourTile, List<ResourceToIntMap> previousIncome, List<ResourceToIntMap> newIncome)
    {
        //Don't do the adjustement if the neighbor is excluded
        if (neighbor.TileData is InfrastructureData)
        {
            foreach (InfrastructureData item in _excludedInfra)
            {
                if (item == neighbor.TileData)
                    return;
            }
        }

        // Apply delta (newIncome - previousIncome)
        behaviourTile.Incomes = Utilities.MergeResourceValues(behaviourTile.Incomes, Utilities.SubtractResourceValues(newIncome, previousIncome));
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
                //Don't do the highlight if the neighbor is excluded
                bool exclude = false;
                if (neighbor.TileData is InfrastructureData)
                {
                    foreach (InfrastructureData item in _excludedInfra)
                    {
                        if (item == neighbor.TileData)
                        {
                            exclude = true;
                            break;
                        }
                    }
                }
                if (exclude)
                    continue;

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        neighbor.Highlight(show);
                }
            }
        }
    }
}

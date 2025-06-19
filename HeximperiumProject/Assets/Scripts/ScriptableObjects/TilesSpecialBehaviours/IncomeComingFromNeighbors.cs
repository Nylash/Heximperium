using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeComingFromneighbors")]
public class IncomeComingFromNeighbors : SpecialBehaviour
{
    [SerializeField] private Resource _resource;
    [SerializeField] private List<TileData> _excludedTiles = new List<TileData>();

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
                if (_excludedTiles.Contains(neighbor.TileData))
                    continue;

                List<ResourceToIntMap> income = new List<ResourceToIntMap>();

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        income.Add(new ResourceToIntMap(_resource, item.value));
                }

                behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, income);

                //Add a lister to adjust the income when a neighbor adjust its own income
                neighbor.OnIncomeModified.RemoveListener(behaviourTile.ListenerOnIncomeModified);
                neighbor.OnIncomeModified.AddListener(behaviourTile.ListenerOnIncomeModified);
            }
            else
            {
                //If the neighbor isn't claimed add a listener to add its income when he will be claimed
                neighbor.OnTileClaimed.RemoveListener(behaviourTile.ListenerOnTileClaimed_IncomeComingFromNeighbors);
                neighbor.OnTileClaimed.AddListener(behaviourTile.ListenerOnTileClaimed_IncomeComingFromNeighbors);
            }
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Nothing needed, replacing by previous tile will be enough (this behaviour only modify its own tile)
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnIncomeModified.RemoveListener(behaviourTile.ListenerOnIncomeModified);
            neighbor.OnTileClaimed.RemoveListener(behaviourTile.ListenerOnTileClaimed_IncomeComingFromNeighbors);
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
                //Don't do the highlight if the neighbor is excluded
                if (_excludedTiles.Contains(neighbor.TileData))
                    continue;

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        neighbor.Highlight(show);
                }
            }
        }
    }

    public void CheckNewIncome(Tile behaviourTile, Tile neighbor, List<ResourceToIntMap> previousIncome, List<ResourceToIntMap> newIncome)
    {
        //Don't do the adjustement if the neighbor is excluded
        if (_excludedTiles.Contains(neighbor.TileData))
            return;

        List<ResourceToIntMap> previousInc = new List<ResourceToIntMap>();
        foreach (ResourceToIntMap item in previousIncome)
        {
            if (item.resource == _resource)
                previousInc.Add(new ResourceToIntMap(_resource, item.value));
        }

        List<ResourceToIntMap> newInc = new List<ResourceToIntMap>();
        foreach (ResourceToIntMap item in newIncome)
        {
            if (item.resource == _resource)
                newInc.Add(new ResourceToIntMap(_resource, item.value));
        }

        // Apply delta (newIncome - previousIncome)
        behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, Utilities.SubtractResourceToIntMaps(newInc, previousInc));
    }

    public void CheckClaimedTile(Tile behaviourTile, Tile tile)
    {
        //Don't do the adjustement if the tile is excluded
        if (_excludedTiles.Contains(tile.TileData))
            return;

        List<ResourceToIntMap> income = new List<ResourceToIntMap>();

        foreach (ResourceToIntMap item in tile.Incomes)
        {
            if (item.resource == _resource)
                income.Add(new ResourceToIntMap(_resource, item.value));
        }

        behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, income);
    }
}

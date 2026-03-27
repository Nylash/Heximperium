using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeComingFromneighbors")]
public class IncomeComingFromNeighbors : SpecialBehaviour
{
    [SerializeField] private Resource _resource;
    [SerializeField] private List<InfrastructureData> _excludedTiles = new List<InfrastructureData>();

    //Subscribe to neighbors event OnIncomeModified and update its income based on their income
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
                //Add a lister to adjust the income when a neighbor adjust its own income or change its data
                neighbor.OnIncomeModified -= behaviourTile.ListenerOnIncomeModified;
                neighbor.OnIncomeModified += behaviourTile.ListenerOnIncomeModified;
                neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_IncomeComingFromNeighbors;
                neighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_IncomeComingFromNeighbors;

                //Don't do the adjustement if the neighbor is excluded
                if (neighbor.TileData is InfrastructureData data && _excludedTiles.Contains(data))
                    continue;

                List<ResourceToIntMap> income = new List<ResourceToIntMap>();

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        income.Add(new ResourceToIntMap(_resource, item.value));
                }

                if (income.Count > 0)
                    behaviourTile.UpdateIncomes(income, true, null, neighbor);
            }
            else
            {
                //If the neighbor isn't claimed add a listener to add its income when he will be claimed
                neighbor.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_IncomeComingFromNeighbors;
                neighbor.OnTileClaimed += behaviourTile.ListenerOnTileClaimed_IncomeComingFromNeighbors;
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
                //Don't do the rollback if the neighbor is excluded
                if (neighbor.TileData is InfrastructureData data && _excludedTiles.Contains(data))
                    continue;

                List<ResourceToIntMap> income = new List<ResourceToIntMap>();

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        income.Add(new ResourceToIntMap(_resource, item.value));
                }

                if (income.Count > 0)
                    behaviourTile.UpdateIncomes(income, false, null, neighbor);
            }

            neighbor.OnIncomeModified -= behaviourTile.ListenerOnIncomeModified;
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_IncomeComingFromNeighbors;
            neighbor.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_IncomeComingFromNeighbors;
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
                //Don't do the highlight if the neighbor is excluded
                if (neighbor.TileData is InfrastructureData data && _excludedTiles.Contains(data))
                    continue;

                foreach (ResourceToIntMap item in neighbor.Incomes)
                {
                    if (item.resource == _resource)
                        neighbor.Highlight(show);
                }
            }
        }
    }

    public void CheckNewData(
        Tile behaviourTile,
        Tile neighbor,
        TileData previousData,
        TileData newData,
        List<ResourceToIntMap> previousIncome,
        List<ResourceToIntMap> newIncome)
    {
        // previousData / newData are TileData
        // -> if cast fail, we know they aren't exlcuded
        bool wasExcluded = previousData is InfrastructureData prevInfra &&
                           _excludedTiles.Contains(prevInfra);

        bool isExcluded = newData is InfrastructureData newInfra &&
                           _excludedTiles.Contains(newInfra);

        // Same status, nothing to do here
        if (wasExcluded == isExcluded)
            return;

        // Get income relative to the right resource
        List<ResourceToIntMap> prevInc = new();
        foreach (var item in previousIncome)
            if (item.resource == _resource)
                prevInc.Add(new ResourceToIntMap(_resource, item.value));

        List<ResourceToIntMap> newInc = new();
        foreach (var item in newIncome)
            if (item.resource == _resource)
                newInc.Add(new ResourceToIntMap(_resource, item.value));

        if (!wasExcluded && isExcluded)
        {
            // Included -> excluded : remove previous income
            if (prevInc.Count > 0)
                behaviourTile.UpdateIncomes(prevInc, false, null, neighbor);
        }
        else if (wasExcluded && !isExcluded)
        {
            // Excluded -> included : add new income
            if (newInc.Count > 0)
                behaviourTile.UpdateIncomes(newInc, true, null, neighbor);
        }
    }

    public void CheckNewIncome(Tile behaviourTile, Tile neighbor, List<ResourceToIntMap> previousIncome, List<ResourceToIntMap> newIncome)
    {
        //Don't do the adjustement if the neighbor is excluded
        if (neighbor.TileData is InfrastructureData data && _excludedTiles.Contains(data))
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

        List<ResourceToIntMap> delta = Utilities.SubtractResourceToIntMaps(newInc, previousInc);
        if (delta.Count > 0)
            behaviourTile.UpdateIncomes(delta, true, null, neighbor);
    }

    public void CheckClaimedTile(Tile behaviourTile, Tile tile)
    {
        //Add a listener to adjust the income when a neighbor adjust its own income or change its data
        tile.OnIncomeModified -= behaviourTile.ListenerOnIncomeModified;
        tile.OnIncomeModified += behaviourTile.ListenerOnIncomeModified;
        tile.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_IncomeComingFromNeighbors;
        tile.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_IncomeComingFromNeighbors;

        //Don't do the adjustement if the tile is excluded
        if (tile.TileData is InfrastructureData data && _excludedTiles.Contains(data))
            return;

        List<ResourceToIntMap> income = new List<ResourceToIntMap>();

        foreach (ResourceToIntMap item in tile.Incomes)
        {
            if (item.resource == _resource)
                income.Add(new ResourceToIntMap(_resource, item.value));
        }

        behaviourTile.UpdateIncomes(income, true, null, tile);
    }

    public override string GetBehaviourDescription()
    {
        string tmp = $"Increases {_resource.ToCustomString()} income by the sum of every neighbors' {_resource.ToCustomString()} income";
        if (_excludedTiles.Count > 0)
            tmp += $" (excluding: {_excludedTiles.ToCustomString(true)})";
        return tmp;
    }
}

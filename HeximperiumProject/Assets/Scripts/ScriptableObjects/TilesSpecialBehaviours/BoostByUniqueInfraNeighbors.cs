using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostByUniqueInfraNeighbors")]
public class BoostByUniqueInfraNeighbors : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _boost = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        //Create a hashset to count unique infra around
        HashSet<InfrastructureData> uniqueData = new HashSet<InfrastructureData>();
        foreach (Tile neighbor in behaviourTile.Neighbors) 
        {
            if (!neighbor) 
                continue;
            if( neighbor.TileData is InfrastructureData data)
                uniqueData.Add(data);

            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors;
            neighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors;
        }

        behaviourTile.UniqueInfraNeighborsCount = uniqueData.Count;

        //Boost for each unique infra
        for (int i = 0; i < behaviourTile.UniqueInfraNeighborsCount; i++)
            behaviourTile.UpdateIncomes(_boost, true);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors;
        }

        //Remove boost for each unique infra
        for (int i = 0; i < behaviourTile.UniqueInfraNeighborsCount; i++)
            behaviourTile.UpdateIncomes(_boost, false);

        behaviourTile.UniqueInfraNeighborsCount = 0;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData)
                neighbor.Highlight(show);
        }
    }

    public void CheckNewData(Tile behaviourTile)
    {
        HashSet<InfrastructureData> uniqueData = new HashSet<InfrastructureData>();
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data)
                uniqueData.Add(data);
        }

        int delta = uniqueData.Count - behaviourTile.UniqueInfraNeighborsCount;

        if (delta == 0)
            return;//Same count, nothing to do

        if (delta > 0)
        {
            // Add boost delta times
            for (int i = 0; i < delta; i++)
                behaviourTile.UpdateIncomes(_boost, true);
        }
        else
        {
            // Remove boost |delta| times
            for (int i = 0; i < -delta; i++)
                behaviourTile.UpdateIncomes(_boost, false);
        }

        behaviourTile.UniqueInfraNeighborsCount = uniqueData.Count;
    }

    public override string GetBehaviourDescription()
    {
        return $"Income boosted by {_boost.IncomeToString()} for each unique neighboring infrastructure";
    }
}

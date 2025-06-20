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

            neighbor.OnTileDataModified.AddListener(behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors);
        }

        behaviourTile.UniqueInfraNeighborsCount = uniqueData.Count;

        //Boost for each unique infra
        for (int i = 0; i < behaviourTile.UniqueInfraNeighborsCount; i++)
            behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _boost);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Create a hashset to count unique infra around
        HashSet<InfrastructureData> uniqueData = new HashSet<InfrastructureData>();
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data)
                uniqueData.Add(data);
        }

        behaviourTile.UniqueInfraNeighborsCount = 0;

        //Remove boost for each unique infra
        for (int i = 0; i < uniqueData.Count; i++)
            behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _boost);
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

        if (behaviourTile.UniqueInfraNeighborsCount == uniqueData.Count)
            return;//Count of unique infra neighbors didn't change, so nothing to do

        for (int i = 0; i < behaviourTile.UniqueInfraNeighborsCount; i++)//Remove previous boosts
            behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _boost);

        behaviourTile.UniqueInfraNeighborsCount = uniqueData.Count;

        for (int i = 0; i < behaviourTile.UniqueInfraNeighborsCount; i++)//Add new boosts
            behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _boost);
    }
}

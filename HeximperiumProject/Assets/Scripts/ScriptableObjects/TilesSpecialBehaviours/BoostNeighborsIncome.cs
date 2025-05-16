using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostNeighborsIncome")]
public class BoostNeighborsIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private InfrastructureData _infrastructureBoosted;

    //Boost the neighbors if it's the right one
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors) 
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData == _infrastructureBoosted)
            {
                neighbor.Incomes = Utilities.MergeResourceValues(neighbor.Incomes, _incomeBoost);
            }
        }
    }

    //Check if the specific tile need the boost
    public override void ApplySpecialBehaviour(Tile specificTile)
    {
        if(specificTile.TileData == _infrastructureBoosted)
        {
            specificTile.Incomes = Utilities.MergeResourceValues(specificTile.Incomes, _incomeBoost);
        }
    }

    //Create a list with -boost and then merge it with the neighbors of the right type
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        List<ResourceToIntMap> tmpList = new List<ResourceToIntMap>();

        foreach (ResourceToIntMap resourceValue in _incomeBoost)
        {
            tmpList.Add(new ResourceToIntMap(resourceValue.resource, -resourceValue.value));
        }
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData == _infrastructureBoosted)
            {
                neighbor.Incomes = Utilities.MergeResourceValues(neighbor.Incomes, tmpList);
            }
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData == _infrastructureBoosted)
            {
                neighbor.BoostHighlight(show);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostNeighborsIncome")]
public class BoostNeighborsIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private List<TileData> _infrastructuresBoosted = new List<TileData>();

    //Boost the neighbors if it's the right one
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors) 
        {
            if (!neighbor)
                continue;
            if (_infrastructuresBoosted.Contains(neighbor.TileData))
            {
                neighbor.Incomes = Utilities.MergeResourceToIntMaps(neighbor.Incomes, _incomeBoost);
            }
        }
    }

    //Check if the specific tile need the boost
    public override void ApplySpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        if(_infrastructuresBoosted.Contains(specificTile.TileData))
        {
            specificTile.Incomes = Utilities.MergeResourceToIntMaps(specificTile.Incomes, _incomeBoost);
        }
    }

    //Remove the income boost from neighbors (if they are boosted)
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_infrastructuresBoosted.Contains(neighbor.TileData))
            {
                neighbor.Incomes = Utilities.SubtractResourceToIntMaps(neighbor.Incomes, _incomeBoost);
            }
        }
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        if (_infrastructuresBoosted.Contains(specificTile.TileData))
        {
            specificTile.Incomes = Utilities.SubtractResourceToIntMaps(specificTile.Incomes, _incomeBoost);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_infrastructuresBoosted.Contains(neighbor.TileData))
            {
                neighbor.Highlight(show);
            }
        }
    }
}

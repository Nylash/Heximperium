using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostNeighborsIncome")]
public class BoostNeighborsIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _infrastructuresBoosted = new List<InfrastructureData>();

    //Boost the neighbors if it's the right one
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors) 
        {
            if (!neighbor)
                continue;
            if (_infrastructuresBoosted.Contains(neighbor.TileData as InfrastructureData))
            {
                neighbor.Incomes = Utilities.MergeResourceValues(neighbor.Incomes, _incomeBoost);
            }
        }
    }

    //Check if the specific tile need the boost
    public override void ApplySpecialBehaviourToSpecificTile(Tile specificTile)
    {
        if(_infrastructuresBoosted.Contains(specificTile.TileData as InfrastructureData))
        {
            specificTile.Incomes = Utilities.MergeResourceValues(specificTile.Incomes, _incomeBoost);
        }
    }

    //Remove the income boost from neighbors (if they are boosted)
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_infrastructuresBoosted.Contains(neighbor.TileData as InfrastructureData))
            {
                neighbor.Incomes = Utilities.SubtractResourceValues(neighbor.Incomes, _incomeBoost);
            }
        }
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile)
    {
        if (_infrastructuresBoosted.Contains(specificTile.TileData as InfrastructureData))
        {
            specificTile.Incomes = Utilities.SubtractResourceValues(specificTile.Incomes, _incomeBoost);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_infrastructuresBoosted.Contains(neighbor.TileData as InfrastructureData))
            {
                neighbor.Highlight(show);
            }
        }
    }
}

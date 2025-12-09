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
            if (neighbor.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
            {
                neighbor.UpdateIncomes(_incomeBoost, true, behaviourTile);
            }
            //BehaviourTile is needed even if the reference isn't in the method to create a unique pair of behaviourTile and neighbor, avoiding conflict between events
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_BoostNeighborsIncome;
            neighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_BoostNeighborsIncome;
        }
    }

    //Remove the income boost from neighbors (if they are boosted)
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
            {
                neighbor.UpdateIncomes(_incomeBoost, false, behaviourTile);
            }
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_BoostNeighborsIncome;
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
            {
                neighbor.Highlight(show);
            }
        }
    }

    public void CheckNewData(Tile tile, Tile behaviourTile)
    {
        if (tile.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
        {
            //Check if the previous data didn't already get the boost
            if (tile.PreviousData is InfrastructureData d && _infrastructuresBoosted.Contains(d))
                return;
            tile.UpdateIncomes(_incomeBoost, true, behaviourTile);
        }
        else
        {
            //Check if the previous data did get a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData d && _infrastructuresBoosted.Contains(d))
                tile.UpdateIncomes(_incomeBoost, false, behaviourTile);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Boosts the income of neighboring {_infrastructuresBoosted.ToCustomString(true)} by {_incomeBoost.IncomeToString()}";
    }
}

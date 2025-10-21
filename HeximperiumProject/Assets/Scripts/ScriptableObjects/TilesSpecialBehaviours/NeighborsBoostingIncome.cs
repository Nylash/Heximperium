using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/NeighborsBoostingIncome")]
public class NeighborsBoostingIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _boostingInfrastructures = new List<InfrastructureData>();

    //Get a boost if the neighbor is right
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data && _boostingInfrastructures.Contains(data))
            {
                behaviourTile.UpdateIncomes(_incomeBoost, true);
            }
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
            neighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
        }
    }

    //Remove a boost if the neighbor is right
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data && _boostingInfrastructures.Contains(data))
            {
                behaviourTile.UpdateIncomes(_incomeBoost, false);
            }
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is InfrastructureData data && _boostingInfrastructures.Contains(data))
            {
                neighbor.Highlight(show);
            }
        }
    }

    public void CheckNewData(Tile behaviourTile, Tile tile)
    {
        if (tile.TileData is InfrastructureData data && _boostingInfrastructures.Contains(data))
        {
            //Check if the previous data didn't already applied the boost
            if (tile.PreviousData is InfrastructureData d && _boostingInfrastructures.Contains(d))
                return;
            behaviourTile.UpdateIncomes(_incomeBoost, true);
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData d && _boostingInfrastructures.Contains(d))
                behaviourTile.UpdateIncomes(_incomeBoost, false);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Income boosted by {_incomeBoost.IncomeToString()} for each neighboring {_boostingInfrastructures.ToCustomString(true, false, false)}";
    }
}

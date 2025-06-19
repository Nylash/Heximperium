using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/NeighborsBoostingIncome")]
public class NeighborsBoostingIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private List<TileData> _boostingInfrastructures = new List<TileData>();

    //Get a boost if the neighbor is right
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_boostingInfrastructures.Contains(neighbor.TileData))
            {
                behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _incomeBoost);
            }
            neighbor.OnTileDataModified.RemoveListener(behaviourTile.CheckNewData);
            neighbor.OnTileDataModified.AddListener(behaviourTile.CheckNewData);
        }
    }

    //Remove a boost if the neighbor is right
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_boostingInfrastructures.Contains(neighbor.TileData))
            {
                behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _incomeBoost);
            }
            neighbor.OnTileDataModified.RemoveListener(behaviourTile.CheckNewData);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (_boostingInfrastructures.Contains(neighbor.TileData))
            {
                neighbor.Highlight(show);
            }
        }
    }

    public void CheckNewData(Tile behaviourTile, Tile tile)
    {
        if (_boostingInfrastructures.Contains(tile.TileData))
        {
            //Check if the previous data didn't already applied the boost
            if (_boostingInfrastructures.Contains(tile.PreviousData))
                return;
            behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _incomeBoost);
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (_boostingInfrastructures.Contains(tile.PreviousData))
                behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _incomeBoost);
        }
    }
}

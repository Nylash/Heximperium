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
            //BehaviourTile is needed even if the reference isn't in the method to create a unique pair of behaviourTile and neighbor, avoiding conflict between events
            neighbor.OnTileDataModified.RemoveListener(behaviourTile.CheckIfBoostNeeded);
            neighbor.OnTileDataModified.AddListener(behaviourTile.CheckIfBoostNeeded);
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
            neighbor.OnTileDataModified.RemoveListener(behaviourTile.CheckIfBoostNeeded);
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

    public void CheckIfBoostNeeded(Tile tile)
    {
        if (_infrastructuresBoosted.Contains(tile.TileData))
        {
            //Check if the previous data didn't already get the boost
            if (_infrastructuresBoosted.Contains(tile.PreviousData))
                return;
            tile.Incomes = Utilities.MergeResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
        else
        {
            //Check if the previous data did get a boost, then remove it if yes
            if (_infrastructuresBoosted.Contains(tile.PreviousData))
                tile.Incomes = Utilities.SubtractResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
    }
}

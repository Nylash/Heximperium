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
        }
    }

    //Check if the specific tile should boost the behaviour tile
    public override void InitializeSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        if(_boostingInfrastructures.Contains(specificTile.TileData))
            behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _incomeBoost);
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
        }
    }

    //Check if the specific tile should remove a boost
    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        if (_boostingInfrastructures.Contains(specificTile.TileData))
            behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _incomeBoost);
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
}

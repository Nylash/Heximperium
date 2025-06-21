using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostNeighborsWithInfraIncome")]
public class BoostNeighborsWithInfraIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                if (neighbor.TileData is InfrastructureData)
                    neighbor.Incomes = Utilities.MergeResourceToIntMaps(neighbor.Incomes, _incomeBoost);
            }

            neighbor.OnTileDataModified.RemoveListener(behaviourTile.ListenerOnTileDataModified_BoostNeighborsWithInfraIncome);
            neighbor.OnTileDataModified.AddListener(behaviourTile.ListenerOnTileDataModified_BoostNeighborsWithInfraIncome);
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                if (neighbor.TileData is InfrastructureData)
                    neighbor.Incomes = Utilities.SubtractResourceToIntMaps(neighbor.Incomes, _incomeBoost);
            }

            neighbor.OnTileDataModified.RemoveListener(behaviourTile.ListenerOnTileDataModified_BoostNeighborsWithInfraIncome);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
            {
                if (neighbor.TileData is InfrastructureData)
                    neighbor.Highlight(show);
            }
        }
    }

    public void CheckNewData(Tile tile)
    {
        if (tile.TileData is InfrastructureData)
        {
            //Check if the previous data didn't already get the boost
            if (tile.PreviousData is InfrastructureData)
                return;
            tile.Incomes = Utilities.MergeResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
        else
        {
            //Check if the previous data did get a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData)
                tile.Incomes = Utilities.SubtractResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
    }
}

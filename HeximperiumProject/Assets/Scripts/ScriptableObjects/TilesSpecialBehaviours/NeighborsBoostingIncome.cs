using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/NeighborsBoostingIncome")]
public class NeighborsBoostingIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _boostingInfrastructures = new List<InfrastructureData>();
    [SerializeField][Range(1, 2)] private int _neighborRange = 1;

    //Get a boost if the neighbor is right
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        // Build O(1) lookup of direct neighbors; seed a 'seen' set to dedupe ring-2.
        HashSet<Tile> directNeighbors = new HashSet<Tile>(behaviourTile.Neighbors.Where(n => n != null));
        HashSet<Tile> seen = new HashSet<Tile> { behaviourTile }; // exclude self
        seen.UnionWith(directNeighbors);                            // exclude ring-1

        // Process ring-1 once each
        foreach (Tile neighbor in directNeighbors)
        {
            if (neighbor.TileData is InfrastructureData d1 && _boostingInfrastructures.Contains(d1))
                behaviourTile.UpdateIncomes(_incomeBoost, true, null, neighbor);

            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
            neighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;

            if (_neighborRange != 2) continue;

            // Collect ring-2 and process each unique tile once
            foreach (Tile secondNeighbor in neighbor.Neighbors)
            {
                if (secondNeighbor == null) continue;
                if (!seen.Add(secondNeighbor)) continue;        // skip self, ring-1, and duplicates

                if (secondNeighbor.TileData is InfrastructureData d2 && _boostingInfrastructures.Contains(d2))
                    behaviourTile.UpdateIncomes(_incomeBoost, true, null, secondNeighbor);

                secondNeighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
                secondNeighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
            }
        }
    }

    //Remove a boost if the neighbor is right
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        // Build O(1) lookup of direct neighbors; seed a 'seen' set to dedupe ring-2.
        HashSet<Tile> directNeighbors = new HashSet<Tile>(behaviourTile.Neighbors.Where(n => n != null));
        HashSet<Tile> seen = new HashSet<Tile> { behaviourTile }; // exclude self
        seen.UnionWith(directNeighbors);                            // exclude ring-1

        // Process ring-1 once each
        foreach (Tile neighbor in directNeighbors)
        {
            if (neighbor.TileData is InfrastructureData d1 && _boostingInfrastructures.Contains(d1))
                behaviourTile.UpdateIncomes(_incomeBoost, false, null, neighbor);

            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;

            if (_neighborRange != 2) continue;

            // Collect ring-2 and process each unique tile once
            foreach (Tile secondNeighbor in neighbor.Neighbors)
            {
                if (secondNeighbor == null) continue;
                if (!seen.Add(secondNeighbor)) continue;        // skip self, ring-1, and duplicates

                if (secondNeighbor.TileData is InfrastructureData d2 && _boostingInfrastructures.Contains(d2))
                    behaviourTile.UpdateIncomes(_incomeBoost, false, null, secondNeighbor);

                secondNeighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_NeighborsBoostingIncome;
            }
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        // Build O(1) lookup of direct neighbors; seed a 'seen' set to dedupe ring-2.
        HashSet<Tile> directNeighbors = new HashSet<Tile>(behaviourTile.Neighbors.Where(n => n != null));
        HashSet<Tile> seen = new HashSet<Tile> { behaviourTile }; // exclude self
        seen.UnionWith(directNeighbors);                            // exclude ring-1

        // Process ring-1 once each
        foreach (Tile neighbor in directNeighbors)
        {
            if (neighbor.TileData is InfrastructureData d1 && _boostingInfrastructures.Contains(d1))
                neighbor.Highlight(show);

            if (_neighborRange != 2) continue;

            // Collect ring-2 and process each unique tile once
            foreach (Tile secondNeighbor in neighbor.Neighbors)
            {
                if (secondNeighbor == null) continue;
                if (!seen.Add(secondNeighbor)) continue;        // skip self, ring-1, and duplicates

                if (secondNeighbor.TileData is InfrastructureData d2 && _boostingInfrastructures.Contains(d2))
                    secondNeighbor.Highlight(show);
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
            behaviourTile.UpdateIncomes(_incomeBoost, true, null, tile);
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData d && _boostingInfrastructures.Contains(d))
                behaviourTile.UpdateIncomes(_incomeBoost, false, null, tile);
        }
    }

    public override string GetBehaviourDescription()
    {
        if (_neighborRange == 2)
            return $"Income boosted by {_incomeBoost.IncomeToString()} for each neighboring {_boostingInfrastructures.ToCustomString(true, false, false)} within 2 tiles";
        return $"Income boosted by {_incomeBoost.IncomeToString()} for each neighboring {_boostingInfrastructures.ToCustomString(true, false, false)}";
    }
}

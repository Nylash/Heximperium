using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostByUniqueInfraNeighbors")]
public class BoostByUniqueInfraNeighbors : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _boost = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        var validNeighbors = new List<Tile>();
        foreach (var neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;

            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors;
            neighbor.OnTileDataModified += behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors;

            if (neighbor.TileData is InfrastructureData)
                validNeighbors.Add(neighbor);
        }

        var groups = validNeighbors.GroupBy(n => (InfrastructureData)n.TileData);
        
        var set = GetSet(behaviourTile);
        set.Clear();
        foreach (var g in groups)
        {
            var refTile = g.First();
            set.Add(refTile);
            behaviourTile.UpdateIncomes(_boost, true, null, refTile);
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;
            neighbor.OnTileDataModified -= behaviourTile.ListenerOnTileDataModified_BoostByUniqueInfraNeighbors;
        }

        if (behaviourTile.UniqueInfraNeighborsByBehaviour.TryGetValue(this, out var set))
        {
            foreach (var t in set)
                behaviourTile.UpdateIncomes(_boost, false, null, t);

            behaviourTile.UniqueInfraNeighborsByBehaviour.Remove(this);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;
            if (neighbor.TileData is InfrastructureData)
                neighbor.Highlight(show);
        }
    }

    public void CheckNewData(Tile behaviourTile)
    {
        var set = GetSet(behaviourTile);
        foreach (var t in set)
            behaviourTile.UpdateIncomes(_boost, false, null, t);
        set.Clear();

        var validNeighbors = new List<Tile>();
        foreach (var neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.TileData is HazardousTileData)
                continue;

            if (neighbor.TileData is InfrastructureData)
                validNeighbors.Add(neighbor);
        }

        var groups = validNeighbors.GroupBy(n => (InfrastructureData)n.TileData);

        foreach (var g in groups)
        {
            var refTile = g.First();
            set.Add(refTile);
            behaviourTile.UpdateIncomes(_boost, true, null, refTile);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Income boosted by {_boost.IncomeToString()} for each unique neighboring infrastructure";
    }

    private HashSet<Tile> GetSet(Tile tile)
    {
        if (!tile.UniqueInfraNeighborsByBehaviour.TryGetValue(this, out var set))
        {
            set = new HashSet<Tile>();
            tile.UniqueInfraNeighborsByBehaviour[this] = set;
        }
        return set;
    }
}

using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/AllowEntertainmentOnTileAndNeighbors")]
public class AllowEntertainmentOnTileAndNeighbors : SpecialBehaviour
{
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.AllowEntertainment = true;
        foreach (Tile tile in behaviourTile.Neighbors)
        {
            if (!tile)
                continue;
            tile.AllowEntertainment = true;
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        if (behaviourTile.Neighbors.All(n =>
            !(n?.TileData?.SpecialBehaviours?.Any(b => b is AllowEntertainmentOnTileAndNeighbors) ?? false)))
        {
            behaviourTile.AllowEntertainment = false;
        }
        foreach (Tile tile in behaviourTile.Neighbors)
        {
            if (tile.Neighbors
                .Where(n => n != null && n != behaviourTile) // skip null and the behaviourTile itself
                .All(n => !n.TileData?.SpecialBehaviours?.Any(b => b is AllowEntertainmentOnTileAndNeighbors) ?? true))
            {
                tile.AllowEntertainment = false;
            }
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.Highlight(show);
        }
    }

    public override string GetBehaviourDescription()
    {
        return "Allow placing Entertainment on this tile and its neighbors";
    }
}

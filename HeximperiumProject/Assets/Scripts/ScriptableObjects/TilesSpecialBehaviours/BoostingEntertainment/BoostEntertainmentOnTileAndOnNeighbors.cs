using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostEntertainmentOnTileAndOnNeighbors")]
public class BoostEntertainmentOnTileAndOnNeighbors : SpecialBehaviour
{
    [SerializeField] int _boost;

    //Init and rollback only need to suscribe to the event, Entertainment are only added at the end, when we cannot build/destroy anymore

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors;
        behaviourTile.OnEntertainmentModified += behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors;
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors;
            neighbor.OnEntertainmentModified += behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors;
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors;
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentOnTileAndOnNeighbors;
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        if (behaviourTile.Entertainment)
        {
            behaviourTile.Highlight(show);
        }
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            neighbor.Highlight(show);
        }
    }

    public void CheckNewEntertainment(Tile modifiedTile, Tile behaviourTile)
    {
        if (modifiedTile.Entertainment != null)
        {
            BoostEntertainment(modifiedTile.Entertainment, Transaction.Gain, behaviourTile);
        }
        //No need to remove the boost on Entertainment suppression, its handled by entertainment destruction
    }

    private void BoostEntertainment(Entertainment ent, Transaction transaction, Tile behaviourTile)
    {
        ent.UpdatePoints(_boost, transaction, false, behaviourTile.TileData);
    }

    public override string GetBehaviourDescription()
    {
        return $"Entertainments on this tile and around gain +{_boost}<sprite name=\"Point_Emoji\">";
    }
}

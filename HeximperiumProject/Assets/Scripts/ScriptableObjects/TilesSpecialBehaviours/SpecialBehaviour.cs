using UnityEngine;

public abstract class SpecialBehaviour : ScriptableObject
{
    //Realize special behaviour as the tile with the behaviour
    public abstract void InitializeSpecialBehaviour(Tile behaviourTile);

    //Realize special behaviour toward a specific tile (e.g. when this specific tile change)
    public abstract void ApplySpecialBehaviour(Tile specificTile);

    //Rollback the behaviour (typically when the infra is destroyed)
    public abstract void RollbackSpecialBehaviour(Tile behaviourTile);

    //Method use to show tiles impacted by the special behaviour
    public abstract void HighlightImpactedTile(Tile behaviourTile, bool show);
}

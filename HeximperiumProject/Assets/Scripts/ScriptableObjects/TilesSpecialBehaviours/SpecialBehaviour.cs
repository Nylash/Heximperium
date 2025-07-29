using UnityEngine;

public abstract class SpecialBehaviour : ScriptableObject
{
    //Realize special behaviour
    public abstract void InitializeSpecialBehaviour(Tile behaviourTile);

    //Rollback the behaviour (typically when the infra is destroyed)
    public abstract void RollbackSpecialBehaviour(Tile behaviourTile);

    //Method use to show tiles impacted by the special behaviour
    public abstract void HighlightImpactedTile(Tile behaviourTile, bool show);

    //Method to get a description of the behaviour
    public abstract string GetBehaviourDescription();
}

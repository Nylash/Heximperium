using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/SpawnScoutOnInit")]
public class SpawnScoutOnInit : SpecialBehaviour
{
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.SpawnScout(behaviourTile, true);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Not needed
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }

    public override string GetBehaviourDescription()
    {
        return "Spawns a scout on the tile when built (doesn't count toward the scout limit)";
    }
}

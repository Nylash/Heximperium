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
}

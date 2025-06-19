using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostScoutsLimit")]
public class BoostScoutsLimit : SpecialBehaviour
{
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.ScoutsLimit++;
    }

    public override void InitializeSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Not needed
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.ScoutsLimit--;
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Not needed
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }
}

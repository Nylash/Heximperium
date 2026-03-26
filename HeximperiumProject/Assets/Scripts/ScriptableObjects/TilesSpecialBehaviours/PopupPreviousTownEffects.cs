using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/PopupPreviousTownEffects")]
public class PopupPreviousTownEffects : SpecialBehaviour
{
    public override string GetBehaviourDescription()
    {
        return "Various effects based on previous Town levels";
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        // Nothing needed, this behaviour is only there for the popup, it doesn't actually impact the tile
    }

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        // Nothing needed, this behaviour is only there for the popup, it doesn't actually impact the tile
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        // Nothing needed, this behaviour is only there for the popup, it doesn't actually impact the tile
    }
}

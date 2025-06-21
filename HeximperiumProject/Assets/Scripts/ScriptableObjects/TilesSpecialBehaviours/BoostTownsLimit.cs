using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostTownsLimit")]
public class BoostTownsLimit : SpecialBehaviour
{
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExploitationManager.Instance.UpdateTownLimit(1);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExploitationManager.Instance.UpdateTownLimit(-1);
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }
}

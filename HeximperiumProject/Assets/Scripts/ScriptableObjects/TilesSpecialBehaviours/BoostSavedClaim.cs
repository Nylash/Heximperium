using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostSavedClaim")]
public class BoostSavedClaim : SpecialBehaviour
{
    [SerializeField] private int _savedClaim;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.SavedClaimPerTurn += _savedClaim;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.SavedClaimPerTurn -= _savedClaim;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }
}

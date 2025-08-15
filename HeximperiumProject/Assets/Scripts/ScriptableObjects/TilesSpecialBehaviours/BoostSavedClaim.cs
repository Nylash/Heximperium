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

    public override string GetBehaviourDescription()
    {
        return $"Boosts saved claim quantity by +{_savedClaim}<sprite name=\"Claim_Emoji\">";
    }
}

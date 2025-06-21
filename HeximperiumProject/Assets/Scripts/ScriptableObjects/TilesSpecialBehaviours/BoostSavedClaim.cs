using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostSavedClaim")]
public class BoostSavedClaim : SpecialBehaviour
{
    [SerializeField] private int _savedClaim;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        for (int i = 0; i < _savedClaim; i++)
            ExpansionManager.Instance.SavedClaimPerTurn++;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        for (int i = 0; i < _savedClaim; i++)
            ExpansionManager.Instance.SavedClaimPerTurn--;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }
}

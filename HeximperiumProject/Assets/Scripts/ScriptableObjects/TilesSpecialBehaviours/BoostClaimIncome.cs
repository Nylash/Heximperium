using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostClaimIncome")]
public class BoostClaimIncome : SpecialBehaviour
{
    [SerializeField] private int _claimQuantity;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.ClaimPerTurn += _claimQuantity;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.ClaimPerTurn -= _claimQuantity;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }
}

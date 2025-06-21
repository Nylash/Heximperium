using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostClaimIncome")]
public class BoostClaimIncome : SpecialBehaviour
{
    [SerializeField] private int _claimQuantity;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        for (int i = 0; i < _claimQuantity; i++)
            ExpansionManager.Instance.ClaimPerTurn++;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        for (int i = 0; i < _claimQuantity; i++)
            ExpansionManager.Instance.ClaimPerTurn--;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }
}

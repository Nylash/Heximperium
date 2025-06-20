using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostClaimIncome")]
public class BoostClaimIncome : SpecialBehaviour
{
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.ClaimPerTurn++;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.ClaimPerTurn--;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }
}

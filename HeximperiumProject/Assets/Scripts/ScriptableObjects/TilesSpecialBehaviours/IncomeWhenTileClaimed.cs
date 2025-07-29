using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomeWhenTileClaimed")]
public class IncomeWhenTileClaimed : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _income = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_IncomeWhenTileClaimed;
        ExpansionManager.Instance.OnTileClaimed += behaviourTile.ListenerOnTileClaimed_IncomeWhenTileClaimed;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.OnTileClaimed -= behaviourTile.ListenerOnTileClaimed_IncomeWhenTileClaimed;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }

    public void TileClaimed(Tile behaviourTile)
    {
        ResourcesManager.Instance.UpdateResource(_income, Transaction.Gain, behaviourTile);
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain {_income.ToCustomString()} when a tile is claimed";
    }
}

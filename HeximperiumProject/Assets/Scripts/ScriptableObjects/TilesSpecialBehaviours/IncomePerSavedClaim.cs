using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/IncomePerSavedClaim")]
public class IncomePerSavedClaim : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _income = new List<ResourceToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.OnClaimSaved -= behaviourTile.ListenerOnClaimSaved;
        ExpansionManager.Instance.OnClaimSaved += behaviourTile.ListenerOnClaimSaved;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExpansionManager.Instance.OnClaimSaved -= behaviourTile.ListenerOnClaimSaved;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }

    public void IncomeForSavedClaim(Tile behaviourTile, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            ResourcesManager.Instance.UpdateResource(_income, Transaction.Gain, behaviourTile);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain {_income.ToCustomString()} for every saved claim";
    }
}

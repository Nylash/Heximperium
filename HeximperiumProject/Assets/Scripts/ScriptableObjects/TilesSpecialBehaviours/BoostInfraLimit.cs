using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostInfraLimit")]
public class BoostInfraLimit : SpecialBehaviour
{
    [SerializeField] private List<InfraDataToIntMap> _boostInfraLimit = new List<InfraDataToIntMap>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (var infraDataToInt in _boostInfraLimit)
        {
            for (int i = 0; i < infraDataToInt.availableCopy ; i++)
                ExploitationManager.Instance.InfraAvailableModify(infraDataToInt.infrastructure, Transaction.Gain);
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (var infraDataToInt in _boostInfraLimit)
        {
            for (int i = 0; i < infraDataToInt.availableCopy; i++)
                ExploitationManager.Instance.InfraAvailableModify(infraDataToInt.infrastructure, Transaction.Spent);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }

    public override string GetBehaviourDescription()
    {
        return $"Increases the available copies of {_boostInfraLimit.ToCustomString()}";
    }
}

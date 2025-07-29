using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/ReduceEntertainmentsGoldCost")]
public class ReduceEntertainmentsGoldCost : SpecialBehaviour
{
    [SerializeField] int _reduction;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ResourcesManager.Instance.EntertainmentGoldReduction += _reduction;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ResourcesManager.Instance.EntertainmentGoldReduction -= _reduction;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }

    public override string GetBehaviourDescription()
    {
        return $"Reduces the cost of entertainments by {_reduction}<sprite name=\"Gold_Emoji\">";
    }
}

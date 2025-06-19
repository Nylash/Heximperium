using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/SpecialResourcesCostReduction")]
public class SpecialResourcesCostReduction : SpecialBehaviour
{
    [SerializeField] private Phase _associatedSystem;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ResourcesManager.Instance.SetSSRReduction(_associatedSystem, 1);
    }

    public override void InitializeSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Nothing needed, this behaviour doesn't impact others tiles
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ResourcesManager.Instance.SetSSRReduction(_associatedSystem, -1);
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Nothing needed, this behaviour doesn't impact others tiles
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed, this behaviour doesn't impact others tiles
    }
}

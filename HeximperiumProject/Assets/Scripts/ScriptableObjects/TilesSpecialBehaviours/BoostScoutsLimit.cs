using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostScoutsLimit")]
public class BoostScoutsLimit : SpecialBehaviour
{
    [SerializeField] private int _scoutsIncrease = 1;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.ScoutsLimit += _scoutsIncrease;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        ExplorationManager.Instance.ScoutsLimit -= _scoutsIncrease;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Not needed
    }

    public override string GetBehaviourDescription()
    {
        return "Increases the limit of scouts by " + _scoutsIncrease;
    }
}

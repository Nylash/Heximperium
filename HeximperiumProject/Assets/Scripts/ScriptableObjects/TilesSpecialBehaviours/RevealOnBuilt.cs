using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/RevealOnBuilt")]
public class RevealOnBuilt : SpecialBehaviour
{
    [SerializeField] private int _revealRadius = 3;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        if (ExploitationManager.Instance.IsPredictingIncome)
            return;

        ExplorationManager.Instance.Reveal(behaviourTile, _revealRadius);
    }

    private void RevealTilesRecursively(Tile currentTile, int depth)
    {
        if (depth <= 0)
        {
            return;
        }

        foreach (Tile neighbor in currentTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Revealed)
            {
                neighbor.RevealTile(false);
            }
            // Recursively reveal the neighbors of the current neighbor
            RevealTilesRecursively(neighbor, depth - 1);
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Not needed, used for town only, and the town can't be unbuilt
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        //Nothing needed
    }

    public override string GetBehaviourDescription()
    {
        return $"Reveals all tiles in a {_revealRadius}-tiles radius.";
    }
}

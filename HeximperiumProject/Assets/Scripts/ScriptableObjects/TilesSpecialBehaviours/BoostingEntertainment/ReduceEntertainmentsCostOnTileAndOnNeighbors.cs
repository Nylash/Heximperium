using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/ReduceEntertainmentsCostOnTileAndOnNeighbors")]
public class ReduceEntertainmentsCostOnTileAndOnNeighbors : SpecialBehaviour
{
    [SerializeField] private int costReductionAmount = 1;

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.CarnivalistCostReduction += costReductionAmount;
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.CarnivalistCostReduction += costReductionAmount;
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.CarnivalistCostReduction -= costReductionAmount;
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.CarnivalistCostReduction -= costReductionAmount;
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.Highlight(show);
        }
    }

    public override string GetBehaviourDescription()
    {
        return "Reduce <sprite name=\"Carnivalist_Emoji\"> cost on this tile and around by " + costReductionAmount;
    }
}

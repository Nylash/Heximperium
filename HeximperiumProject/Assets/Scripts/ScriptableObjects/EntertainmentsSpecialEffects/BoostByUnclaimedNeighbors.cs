using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByUnclaimedNeighbors")]
public class BoostByUnclaimedNeighbors : SpecialEffect
{
    [SerializeField] private int _boostAmount;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        CheckEntertainment(associatedEntertainment);
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        // Nothing to rollback since it only effect this entertainment
    }

    public override void HighlightImpactedEntertainment(Tile associatedTile, bool show)
    {
        foreach (Tile neighbor in associatedTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Claimed)
                neighbor.Highlight(show);
        }
    }

    private void CheckEntertainment(Entertainment associatedEnt)
    {
        int unclaimedNeighbors = 0;
        foreach (Tile neighbor in associatedEnt.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Claimed)
                unclaimedNeighbors++;
        }
        if (unclaimedNeighbors > 0)
            associatedEnt.UpdatePoints(_boostAmount * unclaimedNeighbors, Transaction.Gain);
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boostAmount}<sprite name=\"Point_Emoji\"> for each unrevealed or unclaimed neighbors";
    }
}

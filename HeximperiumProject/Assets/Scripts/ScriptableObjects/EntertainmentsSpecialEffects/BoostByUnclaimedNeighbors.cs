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
        // Don't remove points, destroying the entertainment will remove all its points anyway
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Claimed)
            {
                neighbor.UpdateImpactedEntByTile(associatedEntertainment.Tile, -_boostAmount);
            }
        }
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
        foreach (Tile neighbor in associatedEnt.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Claimed)
            {
                associatedEnt.UpdatePoints(_boostAmount, Transaction.Gain, false, neighbor);
                neighbor.UpdateImpactedEntByTile(associatedEnt.Tile, _boostAmount);
            }
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boostAmount}<sprite name=\"Point_Emoji\"> for each unrevealed or unclaimed neighbors";
    }
}

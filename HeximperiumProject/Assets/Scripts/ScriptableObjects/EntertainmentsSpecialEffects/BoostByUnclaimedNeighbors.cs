using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByUnclaimedNeighbors")]
public class BoostByUnclaimedNeighbors : SpecialEffect
{
    [SerializeField] private int _boostAmount;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (neighbor.Claimed)
                continue;
            neighbor.OnTileClaimed += associatedEntertainment.Tile.ListenerOnTileClaimed_BoostByUnclaimedNeighbors;
            BoostEnt(associatedEntertainment, neighbor, Transaction.Gain);
        }
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        // Don't remove points, destroying the entertainment will remove all its points anyway
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnTileClaimed -= associatedEntertainment.Tile.ListenerOnTileClaimed_BoostByUnclaimedNeighbors;
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

    public void BoostEnt(Entertainment associatedEnt, Tile neighbor, Transaction transaction)
    {
        JuiceManager.Instance.BeginWaveFrom(associatedEnt.Tile);
        associatedEnt.UpdatePoints(_boostAmount, transaction, false, neighbor);
        neighbor.UpdateImpactedEntByTile(associatedEnt.Tile, (transaction == Transaction.Gain) ? _boostAmount : -_boostAmount);
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boostAmount}<sprite name=\"Point_Emoji\"> for each unrevealed or unclaimed neighbors";
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostIfEnoughIdenticalNeighbors")]
public class BoostIfEnoughIdenticalNeighbors : SpecialEffect
{
    [SerializeField] private int _boostAmount;
    [SerializeField] private EntertainmentData _entData;
    [SerializeField] private int _requiredIdenticalNeighbors;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        CheckEntertainment(associatedEntertainment);

        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostIfEnoughIdenticalNeighbors;
            neighbor.OnEntertainmentModified += associatedEntertainment.ListenerOnEntertainmentModified_BoostIfEnoughIdenticalNeighbors;
        }
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        associatedEntertainment.BoostedByIdenticalNeighbors = false;
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostIfEnoughIdenticalNeighbors;
        }
        foreach (Tile item in associatedEntertainment.IdenticalNeighbors)
        {
            item.UpdateImpactedTileByEntertainment(associatedEntertainment.Tile, -_boostAmount);
        }
        associatedEntertainment.IdenticalNeighbors.Clear();
    }

    public override void HighlightImpactedEntertainment(Tile associatedTile, bool show)
    {
        foreach (Tile neighbor in associatedTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            if (neighbor.Entertainment.Data == _entData)
                neighbor.Highlight(show);
        }
    }

    public void CheckEntertainment(Entertainment associatedEnt)
    {
        // First, remove previous boosts from identical neighbors
        foreach (Tile item in associatedEnt.IdenticalNeighbors)
        {
            item.UpdateImpactedTileByEntertainment(associatedEnt.Tile, -_boostAmount);
        }
        associatedEnt.IdenticalNeighbors.Clear();
        HashSet<Entertainment> validNeighbors;
        int identicalNeighborCount = CountIdenticalNeighbors(associatedEnt, out validNeighbors);

        // Check boost condition and apply/remove boost as necessary
        if (!associatedEnt.BoostedByIdenticalNeighbors)
        {
            if (identicalNeighborCount >= _requiredIdenticalNeighbors)
            {
                associatedEnt.UpdatePoints(_boostAmount, Transaction.Gain);
                associatedEnt.BoostedByIdenticalNeighbors = true;
            }
            if (identicalNeighborCount == _requiredIdenticalNeighbors) // Exactly at the threshold, the neighbors all directly impact
            {
                foreach (Entertainment ent in validNeighbors)
                {
                    associatedEnt.IdenticalNeighbors.Add(ent.Tile);
                    ent.Tile.UpdateImpactedTileByEntertainment(associatedEnt.Tile, _boostAmount);
                }
            }
        }
        else
        {
            // If it was already boosted, we need to check if it still has enough neighbors
            if (identicalNeighborCount < _requiredIdenticalNeighbors)
            {
                associatedEnt.UpdatePoints(_boostAmount, Transaction.Spent);
                associatedEnt.BoostedByIdenticalNeighbors = false;
            }
        }
    }

    private int CountIdenticalNeighbors(Entertainment e, out HashSet<Entertainment> validNeighbors)
    {
        validNeighbors = new HashSet<Entertainment>();
        foreach (Tile t in e.Tile.Neighbors)
        {
            if (!t)
                continue;
            if (t?.Entertainment != null && t.Entertainment.Data == _entData)
            {
                validNeighbors.Add(t.Entertainment);
            }
        }
        return validNeighbors.Count;
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boostAmount}<sprite name=\"Point_Emoji\"> if it has at least {_requiredIdenticalNeighbors} other {_entData.Type.ToCustomString()} adjacent";
    }
}

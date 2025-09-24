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
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostIfEnoughIdenticalNeighbors;
        }
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
        if (!associatedEnt.BoostedByIdenticalNeighbors)
        {
            if (CountIdenticalNeighbors(associatedEnt) >= _requiredIdenticalNeighbors)
            {
                associatedEnt.UpdatePoints(_boostAmount, Transaction.Gain);
                associatedEnt.BoostedByIdenticalNeighbors = true;
            }
        }
        else
        {
            // If it was already boosted, we need to check if it still has enough neighbors
            if (CountIdenticalNeighbors(associatedEnt) < _requiredIdenticalNeighbors)
            {
                associatedEnt.UpdatePoints(_boostAmount, Transaction.Spent);
                associatedEnt.BoostedByIdenticalNeighbors = false;
            }
        }
    }

    private int CountIdenticalNeighbors(Entertainment e)
    {
        int count = 0;
        foreach (Tile t in e.Tile.Neighbors)
        {
            if (!t)
                continue;
            if (t?.Entertainment != null && t.Entertainment.Data == _entData)
            {
                count++;
                if (count >= _requiredIdenticalNeighbors) break; // early exit
            }
        }
        return count;
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boostAmount}<sprite name=\"Point_Emoji\"> if it has at least {_requiredIdenticalNeighbors} other {_entData.Type.ToCustomString()} adjacent";
    }
}

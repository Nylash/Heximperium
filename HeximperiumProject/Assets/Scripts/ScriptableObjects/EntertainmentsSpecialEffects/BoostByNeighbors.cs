using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByNeighbors")]
public class BoostByNeighbors : SpecialEffect
{
    [SerializeField] private int _boost;
    [SerializeField] private List<EntertainmentData> _boostingNeighbors = new List<EntertainmentData>();

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByNeighbors;
            neighbor.OnEntertainmentModified += associatedEntertainment.ListenerOnEntertainmentModified_BoostByNeighbors;
            if (!neighbor.Entertainment)
                continue;
            if (_boostingNeighbors.Contains(neighbor.Entertainment.Data))
                associatedEntertainment.UpdatePoints(_boost, Transaction.Gain);
        }
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByNeighbors;
            if (!neighbor.Entertainment)
                continue;
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
            if (_boostingNeighbors.Contains(neighbor.Entertainment.Data))
                neighbor.Highlight(show);
        }
    }

    public void CheckEntertainment(Entertainment associatedEntertainment, Tile tile)
    {
        if(tile.Entertainment != null)
        {
            if (_boostingNeighbors.Contains(tile.Entertainment.Data))
            {
                //Check if the previous data didn't already applied the boost
                if (_boostingNeighbors.Contains(tile.PreviousEntertainmentData))
                    return;
                associatedEntertainment.UpdatePoints(_boost, Transaction.Gain);
            }
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (_boostingNeighbors.Contains(tile.PreviousEntertainmentData))
                associatedEntertainment.UpdatePoints(_boost, Transaction.Spent);
        }
    }
}

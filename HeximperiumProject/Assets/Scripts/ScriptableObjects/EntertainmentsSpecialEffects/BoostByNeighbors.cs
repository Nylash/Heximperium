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
            if (neighbor == null)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(associatedEntertainment.ListenerOnEntertainmentModified);
            neighbor.OnEntertainmentModified.AddListener(associatedEntertainment.ListenerOnEntertainmentModified);
            if (neighbor.Entertainment == null)
                continue;
            if (_boostingNeighbors.Contains(neighbor.Entertainment.Data))
                associatedEntertainment.Points += _boost;
        }
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (neighbor == null)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(associatedEntertainment.ListenerOnEntertainmentModified);
            if (neighbor.Entertainment == null)
                continue;
            if (_boostingNeighbors.Contains(neighbor.Entertainment.Data))
                associatedEntertainment.Points -= _boost;
        }
    }

    public override void HighlightImpactedEntertainment(Tile associatedTile, bool show)
    {
        foreach (Tile neighbor in associatedTile.Neighbors)
        {
            if (neighbor == null)
                continue;
            if (neighbor.Entertainment == null)
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
                associatedEntertainment.Points += _boost;
            }
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (_boostingNeighbors.Contains(tile.PreviousEntertainmentData))
                associatedEntertainment.Points -= _boost;
        }
    }
}

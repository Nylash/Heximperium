using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByUniqueNeighbors")]
public class BoostByUniqueNeighbors : SpecialEffect
{
    [SerializeField] private int _boost;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        //Create a hashset to count unique entertainment around
        HashSet<EntertainmentData> uniqueData = new HashSet<EntertainmentData>();
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;

            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByUniqueNeighbors;
            neighbor.OnEntertainmentModified += associatedEntertainment.ListenerOnEntertainmentModified_BoostByUniqueNeighbors;

            if (!neighbor.Entertainment)
                continue;
            uniqueData.Add(neighbor.Entertainment.Data);
        }

        associatedEntertainment.Tile.UniqueEntertainmentNeighborsCount_SE = uniqueData.Count;

        if(uniqueData.Count > 0)
            associatedEntertainment.UpdatePoints(_boost * uniqueData.Count, Transaction.Gain);
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByUniqueNeighbors;
        }
        associatedEntertainment.Tile.UniqueEntertainmentNeighborsCount_SE = 0;
    }

    public override void HighlightImpactedEntertainment(Tile associatedTile, bool show)
    {
        foreach (Tile neighbor in associatedTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            neighbor.Highlight(show);
        }
    }

    public void CheckEntertainment(Entertainment entertainment)
    {
        HashSet<EntertainmentData> uniqueData = new HashSet<EntertainmentData>();
        foreach (Tile neighbor in entertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            uniqueData.Add(neighbor.Entertainment.Data);
        }

        int deltaCount = uniqueData.Count - entertainment.Tile.UniqueEntertainmentNeighborsCount_SE;

        if (deltaCount == 0)
            return;//Count of unique entertainment neighbors didn't change, so nothing to do

        Transaction t;

        if (deltaCount > 0)
            t = Transaction.Gain;
        else
            t = Transaction.Spent;

        entertainment.UpdatePoints(_boost * Mathf.Abs(deltaCount), t);

        entertainment.Tile.UniqueEntertainmentNeighborsCount_SE = uniqueData.Count;
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boost}<sprite name=\"Point_Emoji\"> for each unique entertainment neighbor";
    }
}

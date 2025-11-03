using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByUniqueNeighbors")]
public class BoostByUniqueNeighbors : SpecialEffect
{
    [SerializeField] private int _boost;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        // Initialize list of valid neighbors
        var validNeighbors = new List<Tile>();
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;

            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByUniqueNeighbors;
            neighbor.OnEntertainmentModified += associatedEntertainment.ListenerOnEntertainmentModified_BoostByUniqueNeighbors;

            if (!neighbor.Entertainment)
                continue;

            validNeighbors.Add(neighbor);
        }

        // Group by EntertainmentData
        var groups = validNeighbors.GroupBy(n => n.Entertainment.Data);

        // Distinct count
        int distinctDataCount = groups.Count();

        // Update neighors that have no twin
        associatedEntertainment.UniqueNeighbors.Clear();
        foreach (var g in groups)
        {
            bool isUnique = g.Count() == 1;
            if (isUnique)
            {
                var n = g.First(); // only one in the group
                n.UpdateImpactedEntertainmentByEntertainment(associatedEntertainment.Tile, _boost);
                associatedEntertainment.UniqueNeighbors.Add(n);
            }
        }

        // Update points and store count
        associatedEntertainment.Tile.UniqueEntertainmentNeighborsCount_SE = distinctDataCount;

        if (distinctDataCount > 0)
            associatedEntertainment.UpdatePoints(_boost * distinctDataCount, Transaction.Gain);
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
        foreach (Tile item in associatedEntertainment.UniqueNeighbors)
        {
            item.UpdateImpactedEntertainmentByEntertainment(associatedEntertainment.Tile, -_boost);
        }
        associatedEntertainment.UniqueNeighbors.Clear();
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

    public void CheckEntertainment(Entertainment associatedEntertainment)
    {
        foreach (Tile item in associatedEntertainment.UniqueNeighbors)
        {
            item.UpdateImpactedEntertainmentByEntertainment(associatedEntertainment.Tile, -_boost);
        }
        associatedEntertainment.UniqueNeighbors.Clear();

        // Initialize list of valid neighbors
        var validNeighbors = new List<Tile>();
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            validNeighbors.Add(neighbor);
        }
        // Group by EntertainmentData
        var groups = validNeighbors.GroupBy(n => n.Entertainment.Data);
        // Distinct count delta
        int deltaCount = groups.Count() - associatedEntertainment.Tile.UniqueEntertainmentNeighborsCount_SE;

        // Update neighors that have no twin
        foreach (var g in groups)
        {
            bool isUnique = g.Count() == 1;
            if (isUnique)
            {
                var n = g.First(); // only one in the group
                n.UpdateImpactedEntertainmentByEntertainment(associatedEntertainment.Tile, _boost);
                associatedEntertainment.UniqueNeighbors.Add(n);
            }
        }

        if (deltaCount == 0)
            return;//Count of unique entertainment neighbors didn't change, so nothing to do

        Transaction t;

        if (deltaCount > 0)
            t = Transaction.Gain;
        else
            t = Transaction.Spent;

        associatedEntertainment.UpdatePoints(_boost * Mathf.Abs(deltaCount), t);

        associatedEntertainment.Tile.UniqueEntertainmentNeighborsCount_SE = groups.Count();
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boost}<sprite name=\"Point_Emoji\"> for each unique entertainment neighbor";
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostEntertainmentByUniqueNeighbors")]
public class BoostEntertainmentByUniqueNeighbors : SpecialBehaviour
{
    [SerializeField] private int _boost;
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors;
        behaviourTile.OnEntertainmentModified += behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors;
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors;
            neighbor.OnEntertainmentModified += behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors;
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors;
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= behaviourTile.ListenerOnEntertainmentModified_BoostEntertainmentByUniqueNeighbors;
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            neighbor.Highlight(show);
        }
    }

    public void CheckNewEntertainment(Tile behaviourTile)
    {
        if (!behaviourTile.Entertainment)//No entertainment to buff, we don't do the behaviour
            return;

        HashSet<EntertainmentData> uniqueData = new HashSet<EntertainmentData>();
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            uniqueData.Add(neighbor.Entertainment.Data);
        }

        int deltaCount = uniqueData.Count - behaviourTile.UniqueEntertainmentNeighborsCount_SB;

        if (deltaCount == 0)
            return;//Count of unique entertainment neighbors didn't change, so nothing to do

        Transaction t;

        if (deltaCount > 0)
            t = Transaction.Gain;
        else
            t = Transaction.Spent;

        behaviourTile.Entertainment.UpdatePoints(_boost * Mathf.Abs(deltaCount), t);

        behaviourTile.UniqueEntertainmentNeighborsCount_SB = uniqueData.Count;
    }
}

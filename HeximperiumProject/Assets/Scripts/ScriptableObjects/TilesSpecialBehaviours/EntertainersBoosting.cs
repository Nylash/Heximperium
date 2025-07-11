using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/EntertainersBoosting")]
public class EntertainersBoosting : SpecialBehaviour
{
    [SerializeField] private List<EntertainerFamily> _boostedFamilies = new List<EntertainerFamily>();
    [SerializeField] private int _bonus;
 
    //If the tile and its neighbors had a unit of the right family apply a boost
    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        if (behaviourTile.Entertainer)
        {
            if (_boostedFamilies.Contains(behaviourTile.Entertainer.EntertainerData.Family))
                behaviourTile.Entertainer.Points += _bonus;
        }
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainer)
                continue;
            if (_boostedFamilies.Contains(neighbor.Entertainer.EntertainerData.Family))
                neighbor.Entertainer.Points += _bonus;
        }
    }

    public override void ApplySpecialBehaviour(Tile specificTile)
    {
        //Nothing needed, this behaviour doesn't impact others tiles, instead it impact entertainers, so a dedicated method is used
    }

    //Method called when a entertainer is spawn
    public void BoostingSpecificEntertainer(Entertainer entertainer)
    {
        if (_boostedFamilies.Contains(entertainer.EntertainerData.Family))
            entertainer.Points += _bonus;
    }

    //Remove the bonuses
    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        if (behaviourTile.Entertainer)
        {
            if (_boostedFamilies.Contains(behaviourTile.Entertainer.EntertainerData.Family))
                behaviourTile.Entertainer.Points -= _bonus;
        }
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainer)
                continue;
            if (_boostedFamilies.Contains(neighbor.Entertainer.EntertainerData.Family))
                neighbor.Entertainer.Points -= _bonus;
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        if (behaviourTile.Entertainer)
        {
            if (_boostedFamilies.Contains(behaviourTile.Entertainer.EntertainerData.Family))
                behaviourTile.BoostHighlight(show);
        }
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainer)
                continue;
            if (_boostedFamilies.Contains(neighbor.Entertainer.EntertainerData.Family))
                neighbor.BoostHighlight(show);
        }
    }
}

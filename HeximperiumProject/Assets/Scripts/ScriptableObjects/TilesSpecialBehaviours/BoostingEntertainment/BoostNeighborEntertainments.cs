using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostNeighborEntertainments")]
public class BoostNeighborEntertainments : SpecialBehaviour
{
    [SerializeField] int _boost;
    [SerializeField] List<EntertainmentData> _boostedEntertainments;
    [Tooltip("Infrastructure that boosts the entertainment points, if null, the boost is applied directly")]
    [SerializeField] InfrastructureData _multiplierInfraOnEmpire;

    //Init and rollback only need to suscribe to the event, Entertainment are only added at the end, when we cannot build/destroy anymore

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.OnEntertainmentModified.RemoveListener(behaviourTile.ListenerOnEntertainmentModified_BoostNeighborEntertainments);
        behaviourTile.OnEntertainmentModified.AddListener(behaviourTile.ListenerOnEntertainmentModified_BoostNeighborEntertainments);
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(behaviourTile.ListenerOnEntertainmentModified_BoostNeighborEntertainments);
            neighbor.OnEntertainmentModified.AddListener(behaviourTile.ListenerOnEntertainmentModified_BoostNeighborEntertainments);
        }
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        behaviourTile.OnEntertainmentModified.RemoveListener(behaviourTile.ListenerOnEntertainmentModified_BoostNeighborEntertainments);
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(behaviourTile.ListenerOnEntertainmentModified_BoostNeighborEntertainments);
        }
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        if (behaviourTile.Entertainment)
        {
            if (_boostedEntertainments.Contains(behaviourTile.Entertainment.Data))
                behaviourTile.Highlight(show);
        }
        foreach (Tile neighbor in behaviourTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            if (_boostedEntertainments.Contains(neighbor.Entertainment.Data))
                neighbor.Highlight(show);
        }
    }

    public void CheckNewEntertainment(Tile modifiedTile)
    {
        if (modifiedTile.Entertainment != null)
        {
            if (_boostedEntertainments.Contains(modifiedTile.Entertainment.Data))
                BoostEntertainment(modifiedTile.Entertainment, Transaction.Gain);
        }
        //No need to remove the boost on Entertainment suppression, its handled by entertainment destruction
    }

    private void BoostEntertainment(Entertainment ent, Transaction transaction)
    {
        if (_multiplierInfraOnEmpire != null)
        {
            int boostMultiplier = 0;
            foreach (Tile t in ExploitationManager.Instance.Infrastructures)
            {
                if (t.TileData == _multiplierInfraOnEmpire)
                    boostMultiplier++;
            }
            ent.UpdatePoints(_boost * boostMultiplier, transaction);
        }
        else
            ent.UpdatePoints(_boost, transaction);
    }
}

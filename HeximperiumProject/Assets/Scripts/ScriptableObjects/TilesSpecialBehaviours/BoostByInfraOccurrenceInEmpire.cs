using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostByInfraOccurrenceInEmpire")]
public class BoostByInfraOccurrenceInEmpire : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _boost = new List<ResourceToIntMap>();
    [SerializeField] private List<InfrastructureData> _tilesBoosting = new List<InfrastructureData>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
            {
                behaviourTile.UpdateIncomes(_boost, true);
            }
        }
        ExploitationManager.Instance.OnInfraBuilded -= behaviourTile.ListenerOnInfraBuilded_BoostByInfraOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraDestroyed -= behaviourTile.ListenerOnInfraDestroyed_BoostByInfraOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraBuilded += behaviourTile.ListenerOnInfraBuilded_BoostByInfraOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraDestroyed += behaviourTile.ListenerOnInfraDestroyed_BoostByInfraOccurrenceInEmpire;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
            {
                behaviourTile.UpdateIncomes(_boost, false);
            }
        }

        ExploitationManager.Instance.OnInfraBuilded -= behaviourTile.ListenerOnInfraBuilded_BoostByInfraOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraDestroyed -= behaviourTile.ListenerOnInfraDestroyed_BoostByInfraOccurrenceInEmpire;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
            {
                tile.Highlight(show);
            }
        }
    }

    public void CheckNewInfra(Tile behaviourTile, Tile tile)
    {
        if(tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
        {        
            //Check if the previous data didn't already applied the boost
            if (tile.PreviousData is InfrastructureData previousData && _tilesBoosting.Contains(previousData))
                return;
            behaviourTile.UpdateIncomes(_boost, true);
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData previousData && _tilesBoosting.Contains(previousData))
                behaviourTile.UpdateIncomes(_boost, false);
        }
    }

    public void CheckDestroyedInfra(Tile behaviourTile, Tile tile)
    {
        if(tile.PreviousData is InfrastructureData data && _tilesBoosting.Contains(data))
        {
            behaviourTile.UpdateIncomes(_boost, false);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Boosts tile income by {_boost.IncomeToString()} for each occurrence of {_tilesBoosting.ToCustomString(true, false, false)} in the empire";
    }
}

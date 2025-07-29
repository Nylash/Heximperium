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
                behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _boost);
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
                behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _boost);
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
            behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _boost);
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData previousData && _tilesBoosting.Contains(previousData))
                behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _boost);
        }
    }

    public void CheckDestroyedInfra(Tile behaviourTile, Tile tile)
    {
        if(tile.PreviousData is InfrastructureData data && _tilesBoosting.Contains(data))
        {
            behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _boost);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Boosts tile income by {_boost.ToCustomString()} for each occurrence of {_tilesBoosting.ToCustomString()} in the empire";
    }
}

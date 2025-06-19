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
        ExploitationManager.Instance.OnInfraBuilded.AddListener(behaviourTile.CheckNewInfra);
        ExploitationManager.Instance.OnInfraDestroyed.AddListener(behaviourTile.CheckDestroyedInfra);
    }

    public override void InitializeSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Nothing needed, every tile modification pass by the empire wide event
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        //Nothing needed this tile only impact itself
        ExploitationManager.Instance.OnInfraBuilded.RemoveListener(behaviourTile.CheckNewInfra);
        ExploitationManager.Instance.OnInfraDestroyed.RemoveListener(behaviourTile.CheckDestroyedInfra);
    }

    public override void RollbackSpecialBehaviourToSpecificTile(Tile specificTile, Tile behaviourTile)
    {
        //Nothing needed, every tile modification pass by the empire wide event
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
        //Check if the previous data didn't already applied the boost
        if (tile.PreviousData is InfrastructureData previousData && _tilesBoosting.Contains(previousData))
            return;

        if(tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
        {
            behaviourTile.Incomes = Utilities.MergeResourceToIntMaps(behaviourTile.Incomes, _boost);
        }
    }

    public void CheckDestroyedInfra(Tile behaviourTile, Tile tile)
    {
        if(tile.PreviousData is InfrastructureData data && _tilesBoosting.Contains(data))
        {
            behaviourTile.Incomes = Utilities.SubtractResourceToIntMaps(behaviourTile.Incomes, _boost);
        }
    }
}

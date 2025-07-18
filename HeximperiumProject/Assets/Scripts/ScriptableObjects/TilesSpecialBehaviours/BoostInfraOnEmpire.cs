using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostInfraOnEmpire")]
public class BoostInfraOnEmpire : SpecialBehaviour
{
    [SerializeField] private List<ResourceToIntMap> _incomeBoost = new List<ResourceToIntMap>();
    [SerializeField] private List<TileData> _infrastructuresBoosted = new List<TileData>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
            {
                tile.Incomes = Utilities.MergeResourceToIntMaps(tile.Incomes, _incomeBoost);
            }
        }
        ExploitationManager.Instance.OnInfraBuilded -= behaviourTile.ListenerOnInfraBuilded_BoostInfraOnEmpire;
        ExploitationManager.Instance.OnInfraDestroyed -= behaviourTile.ListenerOnInfraDestroyed_BoostInfraOnEmpire;
        ExploitationManager.Instance.OnInfraBuilded += behaviourTile.ListenerOnInfraBuilded_BoostInfraOnEmpire;
        ExploitationManager.Instance.OnInfraDestroyed += behaviourTile.ListenerOnInfraDestroyed_BoostInfraOnEmpire;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
            {
                tile.Incomes = Utilities.SubtractResourceToIntMaps(tile.Incomes, _incomeBoost);
            }
        }

        ExploitationManager.Instance.OnInfraBuilded -= behaviourTile.ListenerOnInfraBuilded_BoostInfraOnEmpire;
        ExploitationManager.Instance.OnInfraDestroyed -= behaviourTile.ListenerOnInfraDestroyed_BoostInfraOnEmpire;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
            {
                tile.Highlight(show);
            }
        }
    }

    public void CheckNewInfra(Tile behaviourTile, Tile tile)
    {
        if (tile.TileData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
        {
            //Check if the previous data didn't already get the boost
            if (tile.PreviousData is InfrastructureData previousData && _infrastructuresBoosted.Contains(previousData))
                return;
            tile.Incomes = Utilities.MergeResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
        else
        {
            //Check if the previous data did get a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData previousData && _infrastructuresBoosted.Contains(previousData))
                tile.Incomes = Utilities.SubtractResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
    }

    public void CheckDestroyedInfra(Tile behaviourTile, Tile tile)
    {
        if (tile.PreviousData is InfrastructureData data && _infrastructuresBoosted.Contains(data))
        {
            tile.Incomes = Utilities.SubtractResourceToIntMaps(tile.Incomes, _incomeBoost);
        }
    }
}

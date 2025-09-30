using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Entertainment/AllowEntertainmentOnSpecificInfra")]
public class AllowEntertainmentOnSpecificInfra : UpgradeEffect
{
    [SerializeField] private List<InfrastructureData> _specificTilesAllowingEntertainment = new List<InfrastructureData>();

    public override void ApplyEffect()
    {
        EntertainmentManager.Instance.UpgradeAllowEntOnSpecificInfra = this;

        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
            CheckData(tile);

        ExploitationManager.Instance.OnInfraBuilded += CheckData;
        ExploitationManager.Instance.OnInfraDestroyed += CheckIfRollbackNeeded;
    }

    public void CheckData(Tile tile)
    {
        if (tile.TileData is InfrastructureData infra &&
            _specificTilesAllowingEntertainment.Contains(infra))
        {
            tile.AllowEntertainment = true;
        }
    }

    private void CheckIfRollbackNeeded(Tile tile)
    {
        if (tile.PreviousData is InfrastructureData infra &&
            _specificTilesAllowingEntertainment.Contains(infra))
        {
            if (!(tile.TileData?.SpecialBehaviours?.Any(b => b is AllowEntertainmentOnTileAndNeighbors) ?? false) 
                && tile.Neighbors.All(n => !(n?.TileData?.SpecialBehaviours?.Any(b => b is AllowEntertainmentOnTileAndNeighbors) ?? false))) 
            { 
                tile.AllowEntertainment = false; 
            }
        }
    }

    public override string GetEffectDescription()
    {
        return $"Allow placing Entertainments directly on {_specificTilesAllowingEntertainment.ToCustomString(true)}";
    }
}

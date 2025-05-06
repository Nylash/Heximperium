using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostNeighborsIncome")]
public class BoostNeighborsIncome : SpecialBehaviour
{
    [SerializeField] private List<ResourceValue> _incomeBoost = new List<ResourceValue>();
    [SerializeField] private InfrastructureData _infrastructureBoosted;

    public override void InitializeSpecialBehaviour()
    {
        foreach (Tile neighbor in _tile.Neighbors) 
        {
            if(neighbor.TileData == _infrastructureBoosted)
            {
                neighbor.Incomes = Utilities.MergeResourceValues(neighbor.Incomes, _incomeBoost);
            }
        }
    }

    public override void ApplySpecialBehaviour(Tile specificTile)
    {
        if(specificTile.TileData == _infrastructureBoosted)
        {
            specificTile.Incomes = Utilities.MergeResourceValues(specificTile.Incomes, _incomeBoost);
        }
    }

    public override void RollbackSpecialBehaviour()
    {
        List<ResourceValue> tmpList = new List<ResourceValue>();

        foreach (ResourceValue resourceValue in _incomeBoost)
        {
            tmpList.Add(new ResourceValue(resourceValue.resource, -resourceValue.value));
        }
        foreach (Tile neighbor in _tile.Neighbors)
        {
            if (neighbor.TileData == _infrastructureBoosted)
            {
                neighbor.Incomes = Utilities.MergeResourceValues(neighbor.Incomes, tmpList);
            }
        }
    }
}

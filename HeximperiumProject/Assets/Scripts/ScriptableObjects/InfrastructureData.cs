using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Infrastructure")]
public class InfrastructureData : TileData
{
    [SerializeField] private ResourceCost[] _costs;

    public ResourceCost[] Costs { get => _costs; set => _costs = value; }
}

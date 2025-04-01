using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Resource Tile")]
public class ResourceTileData : TileData
{
    [SerializeField] private Resource _resource;
    [SerializeField] private int _resourceIncome;
}

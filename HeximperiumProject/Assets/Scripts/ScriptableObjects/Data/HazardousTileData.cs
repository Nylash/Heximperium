using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Hazardous Tile")]
public class HazardousTileData : TileData
{
    public override int ClaimCost
    {
        get
        {
            if(ExpansionManager.Instance.UpgradeHazardClaimReduction)
            {
                return 1;
            }
            return base.ClaimCost;
        }
    }
}

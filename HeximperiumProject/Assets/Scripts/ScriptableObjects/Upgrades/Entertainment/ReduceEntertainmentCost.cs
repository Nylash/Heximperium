using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Entertainment/ReduceEntertainmentCost")]
public class ReduceEntertainmentCost : UpgradeEffect
{
    [SerializeField] private EntertainmentData _boostedData;
    [SerializeField] private int _costReduction;

    public override void ApplyEffect()
    {
        _boostedData.CarnivalistCost = Mathf.Max(0, _boostedData.CarnivalistCost - _costReduction);
    }

    public override string GetEffectDescription()
    {
        return $"Reduces {_boostedData.Type.ToCustomString()} cost by {_costReduction}<sprite name=\"Carnivalist_Emoji\">";
    }
}

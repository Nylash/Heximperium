using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Upgrades/Entertainment/AddSpecialEffectToEntertainmentData")]
public class AddSpecialEffectToEntertainmentData : UpgradeEffect
{
    [SerializeField] private List<EntertainmentData> _boostedEnt = new List<EntertainmentData>();
    [SerializeField] private SpecialEffect _specialEffect;

    public override void ApplyEffect()
    {
        foreach (EntertainmentData data in _boostedEnt)
        {
            if (!data.SpecialEffects.Contains(_specialEffect))
                data.SpecialEffects.Add(_specialEffect);
        }
        foreach (Entertainment ent in EntertainmentManager.Instance.Entertainments)
        {
            if (_boostedEnt.Contains(ent.Data))
                _specialEffect.InitializeSpecialEffect(ent);
        }
    }

    public override string GetEffectDescription()
    {
        return 
            $"{_boostedEnt.ToCustomString()} " +
            $"{(_boostedEnt.Count == 1 ? "gains" : "gain")} " +
            $"\"{_specialEffect.GetBehaviourDescription()}\"";

    }
}

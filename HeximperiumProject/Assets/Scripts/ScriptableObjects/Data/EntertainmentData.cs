using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Entertainment")]
public class EntertainmentData : ScriptableObject
{
    [Header("_________________________________________________________")]
    [Header("Mandatory Settings")]
    [SerializeField] private EntertainmentType _type;
    [SerializeField] private int _basePoints;
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();
    [Header("_________________________________________________________")]
    [Header("Optionnal Settings")]
    [SerializeField] private SpecialEffect _specialEffect;

    public EntertainmentType Type { get => _type; }
    public int BasePoints { get => _basePoints; }
    public List<ResourceToIntMap> Costs
    {
        get
        {
            if (ResourcesManager.Instance.EntertainmentGoldReduction == 0)
                return _costs;
            List<ResourceToIntMap> reductedCost = Utilities.CloneResourceToIntMap(_costs);
            foreach (ResourceToIntMap item in reductedCost)
            {
                if (item.resource == Resource.Gold)
                    item.value -= ResourcesManager.Instance.EntertainmentGoldReduction;
                if(item.value < 0)
                    item.value = 0;
            }
            return reductedCost;
        }
    }
    public SpecialEffect SpecialEffect { get => _specialEffect; }
}

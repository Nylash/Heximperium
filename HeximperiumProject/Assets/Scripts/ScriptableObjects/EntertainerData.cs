using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Entertainer")]
public class EntertainerData : ScriptableObject
{
    [SerializeField] private EntertainerType _entertainer;
    [SerializeField] private EntertainerFamily _family;
    [SerializeField] private int _points;
    [SerializeField] private List<EntertainerType> _synergies = new List<EntertainerType>();
    [SerializeField] private List<ResourceToIntMap> _costs = new List<ResourceToIntMap>();

    public List<ResourceToIntMap> Costs { get => _costs; }
    public EntertainerType EntertainerType { get => _entertainer; }
    public EntertainerFamily Family { get => _family; }
    public int Points { get => _points; }
    public List<EntertainerType> Synergies { get => _synergies; }

    public void HighlightSynergyTile(Tile tile, bool show)
    {
        foreach (Tile neighbor in tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainer)
                continue;
            if (_synergies.Contains(neighbor.Entertainer.EntertainerData.EntertainerType))
                neighbor.Highlight(show);
        }
    }
}

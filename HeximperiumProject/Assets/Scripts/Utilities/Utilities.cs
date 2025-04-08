using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Utilities
{
    //Return a list of world position around the tile, depending on how many buttons is needed
    public static List<Vector3> GetInteractionButtonsPosition(Vector3 tilePosition, int buttonsNumber)
    {
        List<Vector3> _positions = new List<Vector3>();
        switch (buttonsNumber)
        {
            case 1:
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z + 1));
                return _positions;
            case 2:
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z + 1));
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z - 1));
                return _positions;
            default:
                Debug.LogError("Interaction are not written for this many buttons : " + buttonsNumber);
                return _positions;
        }
    }

    //Merge two List<ResourceValue>
    public static List<ResourceValue> MergeResourceValues(List<ResourceValue> list1, List<ResourceValue> list2)
    {
        var mergedDictionary = new Dictionary<Resource, int>();

        // Add values from the first list
        foreach (var rv in list1)
        {
            if (mergedDictionary.ContainsKey(rv.resource))
            {
                mergedDictionary[rv.resource] += rv.value;
            }
            else
            {
                mergedDictionary[rv.resource] = rv.value;
            }
        }

        // Add values from the second list
        foreach (var rv in list2)
        {
            if (mergedDictionary.ContainsKey(rv.resource))
            {
                mergedDictionary[rv.resource] += rv.value;
            }
            else
            {
                mergedDictionary[rv.resource] = rv.value;
            }
        }

        // Convert the dictionary back to a list
        return mergedDictionary.Select(kvp => new ResourceValue(kvp.Key, kvp.Value)).ToList();
    }
}

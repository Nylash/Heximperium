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
            case 3:
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z + 1));
                _positions.Add(new Vector3(tilePosition.x + 0.8f, 0.5f, tilePosition.z - 0.8f));
                _positions.Add(new Vector3(tilePosition.x - 0.8f, 0.5f, tilePosition.z - 0.8f));
                return _positions;
            case 4:
                _positions.Add(new Vector3(tilePosition.x + 0.8f, 0.5f, tilePosition.z + 0.8f));
                _positions.Add(new Vector3(tilePosition.x + 0.8f, 0.5f, tilePosition.z - 0.8f));
                _positions.Add(new Vector3(tilePosition.x - 0.8f, 0.5f, tilePosition.z - 0.8f));
                _positions.Add(new Vector3(tilePosition.x - 0.8f, 0.5f, tilePosition.z + 0.8f));
                return _positions;
            case 5:
                _positions.Add(new Vector3(tilePosition.x + 0.55f, 0.5f, tilePosition.z + 1));
                _positions.Add(new Vector3(tilePosition.x + 1, 0.5f, tilePosition.z - 0.2f));
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z - 1));
                _positions.Add(new Vector3(tilePosition.x - 1, 0.5f, tilePosition.z - 0.2f));
                _positions.Add(new Vector3(tilePosition.x - 0.55f, 0.5f, tilePosition.z + 1));
                return _positions;
            case 6:
                _positions.Add(new Vector3(tilePosition.x + 0.55f, 0.5f, tilePosition.z + 1));
                _positions.Add(new Vector3(tilePosition.x + 1, 0.5f, tilePosition.z));
                _positions.Add(new Vector3(tilePosition.x + 0.55f, 0.5f, tilePosition.z - 1));
                _positions.Add(new Vector3(tilePosition.x - 0.55f, 0.5f, tilePosition.z - 1));
                _positions.Add(new Vector3(tilePosition.x - 1, 0.5f, tilePosition.z));
                _positions.Add(new Vector3(tilePosition.x - 0.55f, 0.5f, tilePosition.z + 1));
                return _positions;
            case 7:
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z + 1));
                _positions.Add(new Vector3(tilePosition.x + 0.85f, 0.5f, tilePosition.z + 0.65f));
                _positions.Add(new Vector3(tilePosition.x + 1.15f, 0.5f, tilePosition.z - 0.3f));
                _positions.Add(new Vector3(tilePosition.x + 0.5f, 0.5f, tilePosition.z - 1));
                _positions.Add(new Vector3(tilePosition.x - 0.5f, 0.5f, tilePosition.z - 1));
                _positions.Add(new Vector3(tilePosition.x - 1.15f, 0.5f, tilePosition.z - 0.3f));
                _positions.Add(new Vector3(tilePosition.x - 0.85f, 0.5f, tilePosition.z + 0.65f));
                return _positions;
            case 8:
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z + 1));
                _positions.Add(new Vector3(tilePosition.x + 0.85f, 0.5f, tilePosition.z + 0.85f));
                _positions.Add(new Vector3(tilePosition.x + 1.15f, 0.5f, tilePosition.z));
                _positions.Add(new Vector3(tilePosition.x + 0.85f, 0.5f, tilePosition.z - 0.85f));
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z - 1));
                _positions.Add(new Vector3(tilePosition.x - 0.85f, 0.5f, tilePosition.z - 0.85f));
                _positions.Add(new Vector3(tilePosition.x - 1.15f, 0.5f, tilePosition.z));
                _positions.Add(new Vector3(tilePosition.x - 0.85f, 0.5f, tilePosition.z + 0.85f));
                return _positions;
            default:
                Debug.LogError("Interaction are not written for this many buttons : " + buttonsNumber);
                return _positions;
        }
    }

    public static GameObject CreateInteractionButton(Tile tile, Vector3 positon, Interaction interactionType, InfrastructureData infraData = null, EntertainerData entertainerData = null)
    {
        GameObject button = Object.Instantiate(GameManager.Instance.InteractionPrefab, positon, Quaternion.identity);
        button.GetComponent<UI_InteractionButton>().Initialize(tile, interactionType, infraData, entertainerData);

        return button;
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

#region ENUMS
public enum Phase
{
    Explore, Expand, Exploit, Entertain
}

//The int respect the neighbors order, so simply cast it to int match the good neighbor
public enum Direction
{
    TopRight, Right, BottomRight, BottomLeft, Left, TopLeft
}

public enum Resource
{
    Stone, Essence, Horse, Pigment, Crystal, Emberbone, Gold
}

public enum Transaction
{
    Gain, Spent
}

//Whereas the new income is added (merge) or replace the previous one
public enum TypeIncomeUpgrade
{
    Merge, Replace
}

public enum EntertainerType
{
    Sculptor, Painter, AromaWeaver, Magician, EquestrianDancer, FireEater
}

public enum EntertainerFamily
{
    Artist, Mystic, Performer
}
#endregion

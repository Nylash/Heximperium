using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
                _positions.Add(new Vector3(tilePosition.x - 0.8f, 0.5f, tilePosition.z + 0.8f));
                _positions.Add(new Vector3(tilePosition.x + 0.8f, 0.5f, tilePosition.z + 0.8f));
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z - 1));
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

    //Create a button around a tile
    public static GameObject CreateInteractionButton(Tile tile, Vector3 position, Interaction interactionType, InfrastructureData infraData = null, EntertainerData entertainerData = null)
    {
        GameObject button = GameObject.Instantiate(GameManager.Instance.InteractionPrefab, position, Quaternion.identity);
        button.GetComponent<InteractionButton>().Initialize(tile, interactionType, infraData, entertainerData);

        return button;
    }

    //Merge two List<ResourceToIntMap>
    public static List<ResourceToIntMap> MergeResourceValues(List<ResourceToIntMap> list1, List<ResourceToIntMap> list2)
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
        return mergedDictionary.Select(kvp => new ResourceToIntMap(kvp.Key, kvp.Value)).ToList();
    }

    public static string ToCustomString(this Resource value)
    {
        return value switch
        {
            Resource.Gold => "Gold",
            Resource.SpecialResources => "Special Resources",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
        };
    }

    public static string ToCustomString(this EntertainerType value)
    {
        return value switch
        {
            EntertainerType.Sculptor => "Sculptor",
            EntertainerType.Magician => "Magician",
            EntertainerType.Painter => "Painter",
            EntertainerType.EquestrianDancer => "Equestrian dancer",
            EntertainerType.FireEater => "Fire eater",
            EntertainerType.AromaWeaver => "Aroma weaver",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
        };
    }

    public static string ToCustomString(this Direction value)
    {
        return value switch
        {
            Direction.Left => "West",
            Direction.TopLeft => "Northwest",
            Direction.TopRight => "Northeast",
            Direction.Right => "East",
            Direction.BottomRight => "Southeast",
            Direction.BottomLeft => "Southwest",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
        };
    }

    //Spawn VFX for resources (and points) gain
    public static void PlayResourceGainVFX(Tile tile, GameObject prefab, Material mat, int value)
    {
        GameObject vfx = GameObject.Instantiate(prefab, prefab.transform.position + tile.transform.position, prefab.transform.rotation);

        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();

        particleSystem.emission.SetBurst(0, new ParticleSystem.Burst(0, value));

        vfx.GetComponent<ParticleSystemRenderer>().material = mat;

        particleSystem.Play();
    }

    //Subtract a by b and return the resulting List<ResourceToIntMap>
    public static List<ResourceToIntMap> SubtractResourceValues(List<ResourceToIntMap> a, List<ResourceToIntMap> b)
    {
        var result = new Dictionary<Resource, int>();

        foreach (var item in a)
            result[item.resource] = result.GetValueOrDefault(item.resource, 0) + item.value;

        foreach (var item in b)
            result[item.resource] = result.GetValueOrDefault(item.resource, 0) - item.value;

        return result
            .Where(kvp => kvp.Value != 0)
            .Select(kvp => new ResourceToIntMap(kvp.Key, kvp.Value))
            .ToList();
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
    Gold, SpecialResources
}

public enum Transaction
{
    Gain, Spent
}

public enum Interaction
{
    Claim, Town, Scout, Infrastructure, Destroy, Entertainer
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

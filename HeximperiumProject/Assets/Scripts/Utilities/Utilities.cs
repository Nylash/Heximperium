using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class Utilities
{
    //Return a list of world position around the tile, depending on how many buttons is needed
    public static List<Vector2> GetInteractionButtonsPosition(int quantity)
    {
        List<Vector2> _anchors = new List<Vector2>();
        switch (quantity)
        {
            case 1:
                _anchors.Add(new Vector2(.47f, .53f));
                return _anchors;
            case 2:
                _anchors.Add(new Vector2(.43f, .49f));
                _anchors.Add(new Vector2(.51f, .57f));
                return _anchors;
            case 3:
                _anchors.Add(new Vector2(.39f, .45f));
                _anchors.Add(new Vector2(.47f, .53f));
                _anchors.Add(new Vector2(.55f, .61f));
                return _anchors;
            case 4:
                _anchors.Add(new Vector2(.35f, .41f));
                _anchors.Add(new Vector2(.43f, .49f));
                _anchors.Add(new Vector2(.51f, .57f));
                _anchors.Add(new Vector2(.59f, .65f));
                return _anchors;
            case 5:
                _anchors.Add(new Vector2(.31f, .37f));
                _anchors.Add(new Vector2(.39f, .45f));
                _anchors.Add(new Vector2(.47f, .53f));
                _anchors.Add(new Vector2(.55f, .61f));
                _anchors.Add(new Vector2(.63f, .69f));
                return _anchors;
            case 6:
                _anchors.Add(new Vector2(.27f, .33f));
                _anchors.Add(new Vector2(.35f, .41f));
                _anchors.Add(new Vector2(.43f, .49f));
                _anchors.Add(new Vector2(.51f, .57f));
                _anchors.Add(new Vector2(.59f, .65f));
                _anchors.Add(new Vector2(.67f, .73f));
                return _anchors;
            case 7:
                _anchors.Add(new Vector2(.23f, .29f));
                _anchors.Add(new Vector2(.31f, .37f));
                _anchors.Add(new Vector2(.39f, .45f));
                _anchors.Add(new Vector2(.47f, .53f));
                _anchors.Add(new Vector2(.55f, .61f));
                _anchors.Add(new Vector2(.63f, .69f));
                _anchors.Add(new Vector2(.71f, .77f));
                return _anchors;
            case 8:
                _anchors.Add(new Vector2(.19f, .25f));
                _anchors.Add(new Vector2(.27f, .33f));
                _anchors.Add(new Vector2(.35f, .41f));
                _anchors.Add(new Vector2(.43f, .49f));
                _anchors.Add(new Vector2(.51f, .57f));
                _anchors.Add(new Vector2(.59f, .65f));
                _anchors.Add(new Vector2(.67f, .73f));
                _anchors.Add(new Vector2(.75f, .81f));
                return _anchors;
            default:
                Debug.LogError("Interaction are not written for this many buttons : " + quantity);
                return _anchors;
        }
    }

    //Create a button around a tile
    public static GameObject CreateInteractionButton(Transform parent ,Tile tile, Vector2 anchor, Interaction interactionType, InfrastructureData infraData = null, EntertainerData entertainerData = null)
    {
        GameObject button = GameObject.Instantiate(GameManager.Instance.InteractionPrefab, parent);

        RectTransform rectTransform = button.GetComponent<RectTransform>();

        // Get the current anchor values
        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;

        // Modify only the X values of the anchors
        anchorMin.x = anchor.x;
        anchorMax.x = anchor.y;

        // Set the modified anchor values
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Set the new offsets to match the new anchors
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

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

    public static string ToCustomString(this Resource value)
    {
        return value switch
        {
            Resource.Gold => "Gold",
            Resource.Crystal => "Crystal",
            Resource.Stone => "Stone",
            Resource.Essence => "Floral essence",
            Resource.Horse => "Horse",
            Resource.Pigment => "Pigment",
            Resource.Emberbone => "Emberbone",
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

public enum Biome
{
    Grassland, DeepForest, Mountain, Desert, Swamp, Ice
}
#endregion

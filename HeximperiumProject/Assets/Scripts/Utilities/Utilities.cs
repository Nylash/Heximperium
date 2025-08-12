using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utilities
{
    public static Action OnGameInitialized;

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
                _positions.Add(new Vector3(tilePosition.x - 0.8f, 0.5f, tilePosition.z + 0.8f));
                _positions.Add(new Vector3(tilePosition.x + 0.8f, 0.5f, tilePosition.z + 0.8f));
                _positions.Add(new Vector3(tilePosition.x + 0.8f, 0.5f, tilePosition.z - 0.8f));
                _positions.Add(new Vector3(tilePosition.x - 0.8f, 0.5f, tilePosition.z - 0.8f));
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
    public static GameObject CreateInteractionButton(Tile tile, Vector3 position, Interaction interactionType, InfrastructureData infraData = null, EntertainmentData entertainmentData = null, Scout scout = null)
    {
        GameObject button = GameObject.Instantiate(GameManager.Instance.InteractionPrefab, position, Quaternion.identity);
        button.GetComponent<InteractionButton>().Initialize(tile, interactionType, infraData, entertainmentData, scout);

        return button;
    }

    //Merge two List<ResourceToIntMap>
    public static List<ResourceToIntMap> MergeResourceToIntMaps(List<ResourceToIntMap> list1, List<ResourceToIntMap> list2)
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

    public static string ToCustomString(this Resource value)//The "this" is used to extend the enum Resource with a method
    {
        return value switch
        {
            Resource.Gold => "<sprite name=\"Gold_Emoji\">",
            Resource.SpecialResources => "<sprite name=\"SR_Emoji\">",
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

    public static string ToCustomString(this EntertainmentType value)
    {
        return value switch
        {
            EntertainmentType.TastingPavilion => "Tasting Pavilion",
            EntertainmentType.MinstrelStage => "Minstrel Stage",
            EntertainmentType.ParadeRoute => "Parade Route",
            EntertainmentType.MysticGarden => "Mystic Garden",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
        };
    }

    public static string IncomeToString(this List<ResourceToIntMap> incomes)
    {
        string incomeString = string.Empty;
        for (int i = 0; i < incomes.Count; i++)
        {
            if (i > 0)
                incomeString += " & ";
            incomeString += "+" + incomes[i].value + incomes[i].resource.ToCustomString();
        }
        return incomeString;
    }

    public static string CostToString(this List<ResourceToIntMap> incomes)
    {
        string costString = string.Empty;
        for (int i = 0; i < incomes.Count; i++)
        {
            if (i > 0)
                costString += " & ";
            costString += incomes[i].value + incomes[i].resource.ToCustomString() + "(" + ResourcesManager.Instance.GetResourceStock(incomes[i].resource) + ")";
        }
        return costString;
    }

    public static string ToCustomString<T>(this List<T> data) where T : TileData
    {
        string res = string.Empty;
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
            {
                if (i == data.Count - 1)
                    res += " & ";
                else
                    res += ", ";
            }
            res += data[i].TileName;
        }
        return res;
    }

    public static string ToCustomString(this List<EntertainmentData> entertainments)
    {
        string res = string.Empty;
        for (int i = 0; i < entertainments.Count; i++)
        {
            if (i > 0)
            {
                if (i == entertainments.Count - 1)
                    res += " & ";
                else
                    res += ", ";
            }
            res += entertainments[i].Type.ToCustomString();
        }
        return res;
    }

    //Subtract a by b and return the resulting List<ResourceToIntMap>
    public static List<ResourceToIntMap> SubtractResourceToIntMaps(List<ResourceToIntMap> a, List<ResourceToIntMap> b)
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

    //Clone a List<ResourceToIntMap>
    public static List<ResourceToIntMap> CloneResourceToIntMap(List<ResourceToIntMap> original)
    {
        return original.Select(item => new ResourceToIntMap(item.resource, item.value)).ToList();
    }

    //Reanchor a RectTransform to its current position and size
    public static void ReanchorToCurrentRect(RectTransform rt)
    {
        var parent = rt.parent as RectTransform;
        var ps = parent.rect.size;

        // compute new normalized anchors
        Vector2 aMin = rt.anchorMin + rt.offsetMin / ps;
        Vector2 aMax = rt.anchorMax + rt.offsetMax / ps;

        // apply
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}

#region ENUMS
public enum Phase
{
    //None is used for infra and upgrades not tied to a phase system, this is not directly use in the game turn logic
    Explore, Expand, Exploit, Entertain, None
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
    Claim, Scout, Infrastructure, Destroy, Entertainment, RedirectScout
}

public enum EntertainmentType
{
    MinstrelStage, TastingPavilion, ParadeRoute, MysticGarden
}

public enum UpgradeStatus
{
    LockedByPrerequisites, LockedByExclusive, CantAfford, Unlocked, Unlockable
}
#endregion

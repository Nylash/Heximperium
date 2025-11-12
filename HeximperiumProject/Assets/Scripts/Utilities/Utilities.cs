using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Utilities
{
    public static Action OnGameInitialized;

    #region INTERACTION BUTTONS
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
    #endregion

    #region INCOMES
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

    public static int? GetValueFor(this List<ResourceToIntMap> incomes, Resource resource)
    {
        return incomes.FirstOrDefault(r => r.resource == resource)?.value;
    }

    public static bool AreIncomesEqual(List<ResourceToIntMap> a, List<ResourceToIntMap> b)
    {
        if (a == null || b == null) return a == b;
        if (a.Count != b.Count) return false;

        var groupedA = a.GroupBy(r => r.resource)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.value));
        var groupedB = b.GroupBy(r => r.resource)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.value));

        return groupedA.Count == groupedB.Count &&
               groupedA.All(kvp => groupedB.TryGetValue(kvp.Key, out int v) && v == kvp.Value);
    }
    #endregion

    #region CUSTOM STRINGS
    public static string ToCustomString(this Resource value)//The "this" is used to extend the enum Resource with a method
    {
        return value switch
        {
            Resource.Gold => "<sprite name=\"Gold_Emoji\">",
            Resource.SpecialResources => "<sprite name=\"SR_Emoji\">",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
        };
    }

    public static string ToCustomString(this Family value, bool plural = false)
    {
        if (plural)
        {
            return value switch
            {
                Family.None => "None",
                Family.Town => "<u>Towns</u><sprite name=\"Towns_Emoji\">",
                Family.Farm => "<u>Farms</u><sprite name=\"Farms_Emoji\">",
                Family.Mill => "<u>Mills</u><sprite name=\"Mills_Emoji\">",
                Family.Village => "<u>Villages</u><sprite name=\"Villages_Emoji\">",
                Family.Inn => "<u>Inns</u><sprite name=\"Inns_Emoji\">",
                Family.Lumberyard => "<u>Lumberyards</u><sprite name=\"Lumberyards_Emoji\">",
                Family.Temple => "<u>Temples</u><sprite name=\"Temples_Emoji\">",
                Family.Guild => "<u>Guilds</u><sprite name=\"Guilds_Emoji\">",
                Family.Stonework => "<u>Stonework</u><sprite name=\"Stonework_Emoji\">",
                Family.Entertainment => "<u>Entertainments</u><sprite name=\"Ent_Emoji\">",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
            };
        }
        else
        {
            return value switch
            {
                Family.None => "None",
                Family.Town => "<u>Town</u><sprite name=\"Towns_Emoji\">",
                Family.Farm => "<u>Farm</u><sprite name=\"Farms_Emoji\">",
                Family.Mill => "<u>Mill</u><sprite name=\"Mills_Emoji\">",
                Family.Village => "<u>Village</u><sprite name=\"Villages_Emoji\">",
                Family.Inn => "<u>Inn</u><sprite name=\"Inns_Emoji\">",
                Family.Lumberyard => "<u>Lumberyard</u><sprite name=\"Lumberyards_Emoji\">",
                Family.Temple => "<u>Temple</u><sprite name=\"Temples_Emoji\">",
                Family.Guild => "<u>Guild</u><sprite name=\"Guilds_Emoji\">",
                Family.Stonework => "<u>Stonework</u><sprite name=\"Stonework_Emoji\">",
                Family.Entertainment => "<u>Entertainment</u><sprite name=\"Ent_Emoji\">",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
            };
        }
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

    public static string ToCustomString(this EntertainmentType value, bool underline = true)
    {
        if (underline)
        {
            return value switch
            {
                EntertainmentType.TastingPavilion => "<u>Tasting Pavilion</u>",
                EntertainmentType.MinstrelStage => "<u>Minstrel Stage</u>",
                EntertainmentType.ParadeRoute => "<u>Parade Route</u>",
                EntertainmentType.MysticGarden => "<u>Mystic Garden</u>",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown enum value")
            };
        }
        else
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

    public static string ToCustomString(this List<InfraDataToIntMap> limits)
    {
        string limitsString = string.Empty;
        for (int i = 0; i < limits.Count; i++)
        {
            if (i > 0)
                limitsString += " & ";
            limitsString += "<u>" + limits[i].infrastructure.TileName + "</u>" + " by " + limits[i].availableCopy;
        }
        return limitsString;
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

    public static string ToCustomString<T>(this List<T> data, bool showFamily, bool orInsteadOfAnd = false, bool familyPlural = true) where T : TileData
    {
        if (typeof(T) == typeof(InfrastructureData) && showFamily)
        {
            List<InfrastructureData> infrastructureDatas = (List<InfrastructureData>)(object)data;
            string res = string.Empty;
            HashSet<Family> families = new HashSet<Family>();
            foreach (InfrastructureData d in infrastructureDatas)
                families.Add(d.Family);
            List<Family> familiesList = families.ToList();
            for (int i = 0; i < familiesList.Count; i++)
            {
                if (i > 0)
                {
                    if (i == familiesList.Count - 1)
                    {
                        if (orInsteadOfAnd)
                            res += " or ";
                        else
                            res += " & ";
                    }
                    else
                        res += ", ";
                }
                res += familiesList[i].ToCustomString(familyPlural);
            }
            return res;
        }
        else
        {
            string res = string.Empty;
            for (int i = 0; i < data.Count; i++)
            {
                if (i > 0)
                {
                    if (i == data.Count - 1)
                    {
                        if (orInsteadOfAnd)
                            res += " or ";
                        else
                            res += " & ";
                    }
                    else
                        res += ", ";
                }
                res += "<u>" + data[i].TileName + "</u>";
            }
            return res;
        }
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
    #endregion

    #region POPUP
    public static void AnchorWrapperToContent(RectTransform wrapper, RectTransform content)
    {
        // 1) Forcer le layout pour avoir les bonnes tailles
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(wrapper);

        var parent = wrapper.parent as RectTransform;
        if (parent == null) return;

        // 2) Bounding box du content relative au parent du wrapper
        //    (=> coordonn�es locales du parent)
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, content);

        // 3) Convertir en ancres normalis�es [0..1] (origine = coin bas-gauche du parent)
        Vector2 parentSize = parent.rect.size;
        Vector2 parentBL = -Vector2.Scale(parentSize, parent.pivot); // coin bas-gauche en local

        Vector2 minNorm = new Vector2(
            (b.min.x - parentBL.x) / parentSize.x,
            (b.min.y - parentBL.y) / parentSize.y
        );
        Vector2 maxNorm = new Vector2(
            (b.max.x - parentBL.x) / parentSize.x,
            (b.max.y - parentBL.y) / parentSize.y
        );

        // 4) Appliquer aux ancres du wrapper et neutraliser offsets
        wrapper.anchorMin = minNorm;
        wrapper.anchorMax = maxNorm;
        wrapper.anchoredPosition = Vector2.zero;
        wrapper.sizeDelta = Vector2.zero;
    }

    public static void PlacePrefabAroundTargetTopRight(RectTransform prefabRect, RectTransform targetRect, Vector2 offsetNorm)
    {
        // 1) Parent commun (le parent immédiat du prefab)
        var parentRect = (RectTransform)prefabRect.parent;

        // 2) Récupérer le point monde du coin haut-droit de la cible
        var corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2];

        // 3) Convertir ce point en local du parent du prefab
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, worldTopRight),
            null,
            out localPoint
        );

        // 4) Convertir en coordonnées normalisées [0..1] dans le parent
        Vector2 parentSize = parentRect.rect.size;
        Vector2 parentBL = -parentSize * parentRect.pivot; // bas-gauche du parent en local
        Vector2 normalized = (localPoint - parentBL);
        normalized.x /= parentSize.x;
        normalized.y /= parentSize.y;

        // 5) Appliquer l’offset en coordonnées normalisées
        normalized += offsetNorm;

        // 6) Conserver l’écart d’ancres (span) et recentrer sur normalized
        Vector2 span = prefabRect.anchorMax - prefabRect.anchorMin;
        Vector2 half = span * 0.5f;

        Vector2 newMin = normalized - half;
        Vector2 newMax = normalized + half;

        // 7) Clamp [0,1]
        newMin.x = Mathf.Clamp01(newMin.x);
        newMin.y = Mathf.Clamp01(newMin.y);
        newMax.x = Mathf.Clamp01(newMax.x);
        newMax.y = Mathf.Clamp01(newMax.y);

        // 8) Appliquer
        prefabRect.anchorMin = newMin;
        prefabRect.anchorMax = newMax;
        prefabRect.anchoredPosition = Vector2.zero;
    }

    public static bool IsUnderlined(TMP_Text text, int firstChar, int lastChar)
    {
        // Component-level underline?
        if ((text.fontStyle & FontStyles.Underline) != 0) return true;

        var chars = text.textInfo.characterInfo;
        for (int i = firstChar; i <= lastChar; i++)
        {
            if (chars[i].elementType == TMP_TextElementType.Character &&
                (chars[i].style & FontStyles.Underline) != 0)
                return true;
        }
        return false;
    }

    public static string ExtractWholeUnderlinedWord(TMP_TextInfo textInfo, int index)
    {
        var chars = textInfo.characterInfo;
        if (index < 0 || index >= chars.Length)
            return null;

        int start = index;
        int end = index;

        // �tendre � gauche
        while (start > 0 &&
               chars[start - 1].elementType == TMP_TextElementType.Character &&
               (chars[start - 1].style & FontStyles.Underline) != 0)
        {
            start--;
        }

        // �tendre � droite
        while (end < chars.Length - 1 &&
               chars[end + 1].elementType == TMP_TextElementType.Character &&
               (chars[end + 1].style & FontStyles.Underline) != 0)
        {
            end++;
        }

        // Construire le mot complet
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = start; i <= end; i++)
            sb.Append(chars[i].character);

        return sb.ToString();
    }

    public static bool Matches(string s, string valeur, bool falseIfSpace)
    {
        // Mot entier, optionnellement suivi de 's' ou 'es'
        string pattern = $@"\b{Regex.Escape(valeur)}(es|s)?\b";
        bool res = Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase);
        if (falseIfSpace)
        {
            if (s.Contains(' '))
                return false;
        }
        return res;
    }
    #endregion
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

public enum ExtendedResource
{
    Gold, SpecialResources, Claim, Carnivalist
}

public enum Transaction
{
    Gain, Spent
}

public enum Interaction
{
    Claim, Scout, Infrastructure, Destroy, Entertainment, RedirectScout, RevealAnywhere
}

public enum EntertainmentType
{
    MinstrelStage, TastingPavilion, ParadeRoute, MysticGarden
}

public enum UpgradeStatus
{
    LockedByPrerequisites, LockedByExclusive, CantAfford, Unlocked, Unlockable
}

public enum TileInteractionAnimationState
{
    None, Animating, Stopping
}

public enum Family
{
    None, Town, Farm, Mill, Village, Inn, Lumberyard, Temple, Guild, Stonework, Entertainment
}
#endregion

#region STRUCTS
public struct VectorPair
{
    public Vector3 from;
    public Vector3 to;
    public VectorPair(Vector3 from, Vector3 to)
    {
        this.from = from;
        this.to = to;
    }
}
#endregion
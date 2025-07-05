using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByZoneSize")]
public class BoostByZoneSize : SpecialEffect
{
    [SerializeField] private int _boost;
    [SerializeField] private EntertainmentData _dataBoosting;
    [SerializeField] private EntertainmentData _dataBridge;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        HashSet<int> neighborGroups = new HashSet<int>();

        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize);
            neighbor.OnEntertainmentModified.AddListener(associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize);
            if (neighbor.GroupID > 0)
                neighborGroups.Add(neighbor.GroupID);
        }

        if (neighborGroups.Count == 0)
        {
            CreateNewGroup(associatedEntertainment);
        }
        else if (neighborGroups.Count == 1)
        {
            AddEntertainmentToGroup(neighborGroups.First(), associatedEntertainment);
        }
        else
        {
            MergeGroups(neighborGroups.ToList(), associatedEntertainment);
        }
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize);
        }
        RemoveEntertainmentFromItsGroup(associatedEntertainment.Tile, associatedEntertainment.Data);
    }

    public override void HighlightImpactedEntertainment(Tile associatedTile, bool show)
    {
        //TO DO
    }

    //The event logic only needs to check the bridge data, the matching data is handled by the special effect directly
    public void CheckEntertainment(Entertainment associatedEntertainment, Tile updatedTile)
    {
        if (!updatedTile.Entertainment)
        {
            if (updatedTile.PreviousEntertainmentData == _dataBridge)//If it was a bridge remove it
                RemoveEntertainmentFromItsGroup(updatedTile, updatedTile.PreviousEntertainmentData);
        }
        else
        {
            if (updatedTile.Entertainment.Data == _dataBridge)//If it is a bridge add it
            {
                HashSet<int> neighborGroups = new HashSet<int>();

                foreach (Tile neighbor in updatedTile.Neighbors)
                {
                    if (!neighbor)
                        continue;
                    if (neighbor.GroupID > 0)
                        neighborGroups.Add(neighbor.GroupID);
                }

                if (neighborGroups.Count == 0)
                {
                    Debug.LogError("Not possible, listener not removed ? AssociatedEntertainment : " + associatedEntertainment + " UpdatedTile : " + updatedTile);
                }
                else if (neighborGroups.Count == 1)
                {
                    AddEntertainmentToGroup(neighborGroups.First(), updatedTile.Entertainment);
                }
                else
                {
                    MergeGroups(neighborGroups.ToList(), updatedTile.Entertainment);
                }
            }
        }
    }


    private void CreateNewGroup(Entertainment initialEntertainment)
    {
        int newGroupId = 1;
        while (EntertainmentManager.Instance.GroupBoost.ContainsKey(newGroupId))
            newGroupId++;

        List<Entertainment> entertainments = new List<Entertainment>();
        entertainments.Add(initialEntertainment);

        EntertainmentManager.Instance.GroupBoost[newGroupId] = entertainments;
        if(initialEntertainment.Data == _dataBoosting)
            EntertainmentManager.Instance.GroupBoostCount[newGroupId] = 1;
        else
            EntertainmentManager.Instance.GroupBoostCount[newGroupId] = 0;

        initialEntertainment.Tile.GroupID = newGroupId;
    }

    private void AddEntertainmentToGroup(int groupID, Entertainment newEntertainment)
    {
        if (newEntertainment.Data == _dataBoosting)
        {
            foreach (Entertainment item in EntertainmentManager.Instance.GroupBoost[groupID])
            {
                if (item.Data != _dataBoosting)
                    continue;
                item.UpdatePoints(_boost, Transaction.Gain);
            }
        }
            
        EntertainmentManager.Instance.GroupBoost[groupID].Add(newEntertainment);
        if (newEntertainment.Data == _dataBoosting)
        {
            //Apply the boost before increasing the count because the effect shouldn't count itself
            newEntertainment.UpdatePoints(_boost * EntertainmentManager.Instance.GroupBoostCount[groupID], Transaction.Gain);
            EntertainmentManager.Instance.GroupBoostCount[groupID]++;
        }

        newEntertainment.Tile.GroupID = groupID;
    }

    private void RemoveEntertainmentFromItsGroup(Tile tile, EntertainmentData removedData)
    {
        if (removedData == _dataBoosting)
        {
            EntertainmentManager.Instance.GroupBoostCount[tile.GroupID]--;
        }

        if (tile.Entertainment != null)
            EntertainmentManager.Instance.GroupBoost[tile.GroupID].Remove(tile.Entertainment);

        if (removedData == _dataBoosting)
        {
            foreach (Entertainment item in EntertainmentManager.Instance.GroupBoost[tile.GroupID])
            {
                if (item.Data != _dataBoosting)
                    continue;
                item.UpdatePoints(_boost, Transaction.Spent);
            }
        }

        //If there is no more entertainment in the group, remove it
        if (EntertainmentManager.Instance.GroupBoost[tile.GroupID].Count == 0)
        {
            EntertainmentManager.Instance.GroupBoost.Remove(tile.GroupID);
            EntertainmentManager.Instance.GroupBoostCount.Remove(tile.GroupID);
        }  

        tile.GroupID = 0;
    }

    private void MergeGroups(List<int> groupIds, Entertainment newEntertainment)
    {
        // choose the smallest group ID as the base
        int targetGroupId = groupIds.Min();

        // merge all other groups into it
        foreach (int sourceGroupId in groupIds)
        {
            if (sourceGroupId == targetGroupId)
                continue;

            List<Entertainment> sourceList = EntertainmentManager.Instance.GroupBoost[sourceGroupId].ToList(); // copy to avoid modifying during loop

            foreach (Entertainment ent in sourceList)
            {
                ResetEntertainmentPoints(ent);
                RemoveEntertainmentFromItsGroup(ent.Tile, ent.Data);
                AddEntertainmentToGroup(targetGroupId, ent);
            }
            EntertainmentManager.Instance.GroupBoost.Remove(sourceGroupId);
            EntertainmentManager.Instance.GroupBoostCount.Remove(sourceGroupId);
        }

        AddEntertainmentToGroup(targetGroupId, newEntertainment);
    }

    private void ResetEntertainmentPoints(Entertainment entertainment)
    {
        if (entertainment.Data != _dataBoosting)
            return;
        entertainment.UpdatePoints(
            _boost * 
            (EntertainmentManager.Instance.GroupBoostCount[entertainment.Tile.GroupID] -1),//subtract 1 from group count since the boost logic does not count itself
            Transaction.Spent);
    }
}

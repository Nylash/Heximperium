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
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize;
            neighbor.OnEntertainmentModified += associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize;
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

        //If this new entertainment connect with isolated bridge we connect them
        AddUnconnectedBridge(associatedEntertainment);
    }

    public override void RollbackSpecialEntertainment(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified -= associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize;
        }
        foreach (Entertainment isolatedBridge in CheckIsolatedBridges(associatedEntertainment))
            RemoveEntertainmentFromItsGroup(isolatedBridge.Tile, isolatedBridge.Data, false);
        RemoveEntertainmentFromItsGroup(associatedEntertainment.Tile, associatedEntertainment.Data);
    }

    public override void HighlightImpactedEntertainment(Tile associatedTile, bool show)
    {
        HashSet<int> groupIDs = new HashSet<int>();
        foreach (Tile neighbor in associatedTile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            if (neighbor.GroupID > 0)
            {
                groupIDs.Add(neighbor.GroupID);
                continue;
            }
            if (neighbor.Entertainment.Data == _dataBridge)
                neighbor.Highlight(show);
        }
        foreach (int ID in groupIDs)
        {
            foreach (Entertainment ent in EntertainmentManager.Instance.GroupBoost[ID])
                ent.Tile.Highlight(show);
        }
    }

    //The event logic only needs to check the bridge data, the boosting data is handled by the special effect directly
    public void CheckEntertainment(Entertainment associatedEntertainment, Tile updatedTile)
    {
        if (!updatedTile.Entertainment)
        {
            if (updatedTile.PreviousEntertainmentData == _dataBridge && updatedTile.GroupID != 0)//If it was a bridge remove it (and not handled by another listener)
                RemoveEntertainmentFromItsGroup(updatedTile, updatedTile.PreviousEntertainmentData);
        }
        else
        {
            if (updatedTile.Entertainment.Data == _dataBridge)//If it is a bridge add it
            {
                if (updatedTile.GroupID != 0)//Already handled by another listener ?
                    return;

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


    private int CreateNewGroup(Entertainment initialEntertainment)
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
        return newGroupId;
    }

    private void AddEntertainmentToGroup(int groupID, Entertainment newEntertainment, bool skipVFX = false)
    {
        if (newEntertainment.Data == _dataBoosting)
        {
            foreach (Entertainment item in EntertainmentManager.Instance.GroupBoost[groupID])
            {
                if (item.Data != _dataBoosting)
                    continue;
                item.UpdatePoints(_boost, Transaction.Gain, skipVFX);
            }
        }
            
        EntertainmentManager.Instance.GroupBoost[groupID].Add(newEntertainment);
        if (newEntertainment.Data == _dataBoosting)
        {
            //Apply the boost before increasing the count because the effect shouldn't count itself
            newEntertainment.UpdatePoints(_boost * EntertainmentManager.Instance.GroupBoostCount[groupID], Transaction.Gain, skipVFX);
            EntertainmentManager.Instance.GroupBoostCount[groupID]++;
        }

        newEntertainment.Tile.GroupID = groupID;
    }

    private void RemoveEntertainmentFromItsGroup(Tile tile, EntertainmentData removedData, bool checkForSplit = true)
    {
        if (removedData == _dataBoosting)
        {
            EntertainmentManager.Instance.GroupBoostCount[tile.GroupID]--;
        }

        if(tile.Entertainment != null)
            EntertainmentManager.Instance.GroupBoost[tile.GroupID].Remove(tile.Entertainment);//When called by Rollback
        else
            EntertainmentManager.Instance.GroupBoost[tile.GroupID].Remove(tile.PreviousEntertainment);//When called by the event, so after the entertainment is set to null

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
        else if(checkForSplit)
        {
            if (!CheckIfGroupStillWhole(tile.GroupID))
                SplitGroup(tile.GroupID);
        }

        tile.GroupID = 0;
    }

    private void AddUnconnectedBridge(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            if (neighbor.Entertainment.Data == _dataBridge)
            {
                if (neighbor.GroupID == 0)
                    AddEntertainmentToGroup(associatedEntertainment.Tile.GroupID, neighbor.Entertainment);
            }
        }
    }

    private List<Entertainment> CheckIsolatedBridges(Entertainment removedEntertainment)
    {
        List<Entertainment> isolatedBridges = new List<Entertainment>();
        foreach (Tile neighbor in removedEntertainment.Tile.Neighbors)
        {
            bool isIsolated = true;
            if (!neighbor)
                continue;
            if (!neighbor.Entertainment)
                continue;
            if (neighbor.Entertainment.Data != _dataBridge)
                continue;
            foreach (Tile n in neighbor.Neighbors)
            {
                if (!n)
                    continue;
                if (!n.Entertainment)
                    continue;
                if (n.Entertainment == removedEntertainment)
                    continue;
                if (n.Entertainment.Data == _dataBoosting)
                {
                    isIsolated = false;
                    break;
                }
            }
            if(isIsolated)
                isolatedBridges.Add(neighbor.Entertainment);
        }
        return isolatedBridges;
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
                RemoveEntertainmentFromItsGroup(ent.Tile, ent.Data, false);
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

    private bool CheckIfGroupStillWhole(int groupID)
    {
        //Breadth-first search logic
        HashSet<Entertainment> visited = new HashSet<Entertainment>();
        Queue<Entertainment> queue = new Queue<Entertainment>();
        Entertainment start = EntertainmentManager.Instance.GroupBoost[groupID].First();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Entertainment current = queue.Dequeue();
            foreach (Tile neighbor in current.Tile.Neighbors)
            {
                if(!neighbor)
                    continue;
                if(!neighbor.Entertainment)
                    continue;
                if (neighbor.Entertainment.Data != _dataBoosting && neighbor.Entertainment.Data != _dataBridge)
                    continue;
                if (EntertainmentManager.Instance.GroupBoost[groupID].Contains(neighbor.Entertainment) && !visited.Contains(neighbor.Entertainment))
                {
                    visited.Add(neighbor.Entertainment);
                    queue.Enqueue(neighbor.Entertainment);
                }
            }
        }

        return visited.Count == EntertainmentManager.Instance.GroupBoost[groupID].Count;
    }

    private void SplitGroup(int groupID)
    {
        // Fast lookup for remaining tiles
        HashSet<Entertainment> remaining = new HashSet<Entertainment>(EntertainmentManager.Instance.GroupBoost[groupID]);
        List<List<Entertainment>> splittedGroups = new List<List<Entertainment>>();

        // Until we have partitioned every tile
        while (remaining.Count > 0)
        {
            // Start a new component from an arbitrary tile
            List<Entertainment> component = new List<Entertainment>();
            Queue<Entertainment> queue = new Queue<Entertainment>();
            Entertainment start = remaining.First();

            queue.Enqueue(start);
            remaining.Remove(start);
            component.Add(start);

            // BFS to collect all connected tiles in this component
            while (queue.Count > 0)
            {
                Entertainment current = queue.Dequeue();
                foreach (Tile neighbor in current.Tile.Neighbors)
                {
                    if (!neighbor)
                        continue;
                    if (!neighbor.Entertainment)
                        continue;
                    if (neighbor.Entertainment.Data != _dataBoosting && neighbor.Entertainment.Data != _dataBridge)
                        continue;
                    // Only consider tiles still in remaining
                    if (remaining.Remove(neighbor.Entertainment))
                    {
                        queue.Enqueue(neighbor.Entertainment);
                        component.Add(neighbor.Entertainment);
                    }
                }
            }
            splittedGroups.Add(component);
        }

        if (splittedGroups.Count > 1)
        {
            bool isFirst;
            int newGroupID = 0;
            foreach (List<Entertainment> group in splittedGroups)
            {
                isFirst = true;
                foreach (Entertainment ent in group)
                {
                    ResetEntertainmentPoints(ent);
                    RemoveEntertainmentFromItsGroup(ent.Tile, ent.Data, false);
                    if (isFirst)
                    {
                        newGroupID = CreateNewGroup(ent);
                        isFirst = false;
                    }
                    else
                        AddEntertainmentToGroup(newGroupID, ent, true);//Skip VFX too avoid confusing the player
                }
            }
        }
        else
        {
            Debug.LogError("CheckIfGroupStillWhole shouldn't have return false, groupID : " + groupID);
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boost}<sprite name=\"Point_Emoji\"> for each other {_dataBoosting.Type.ToCustomString()} connected to this one through a continuous zone. " +
               $"A {_dataBridge.Type.ToCustomString()} can bridge a one gap tile between two {_dataBoosting.Type.ToCustomString()}";
    }
}

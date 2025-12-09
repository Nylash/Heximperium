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
                    if (!neighbor.Entertainment)
                        continue;
                    if (neighbor.Entertainment.Data != _dataBoosting)
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
                item.UpdatePoints(_boost, Transaction.Gain, skipVFX, newEntertainment.Tile);
                item.Tile.UpdateImpactedEntertainmentByEntertainment(newEntertainment.Tile, _boost);
                newEntertainment.Tile.UpdateImpactedEntertainmentByEntertainment(item.Tile, _boost);//Update the impacted tiles for both entertainments here (to use only one loop)
            }
        }
            
        EntertainmentManager.Instance.GroupBoost[groupID].Add(newEntertainment);
        if (newEntertainment.Data == _dataBoosting)
        {
            //Apply the boost before increasing the count because the effect shouldn't count itself
            foreach (Entertainment ent in EntertainmentManager.Instance.GroupBoost[groupID])
            {
                if (ent == newEntertainment)
                    continue;
                if (ent.Data != _dataBoosting)
                    continue;
                newEntertainment.UpdatePoints(_boost, Transaction.Gain, skipVFX, ent.Tile);
            }
        }

        newEntertainment.Tile.GroupID = groupID;
    }

    private void RemoveEntertainmentFromItsGroup(Tile tile, EntertainmentData removedData, bool checkForSplit = true)
    {
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
                item.UpdatePoints(_boost, Transaction.Spent, false, tile);
                item.Tile.UpdateImpactedEntertainmentByEntertainment(tile, -_boost);
                tile.UpdateImpactedEntertainmentByEntertainment(item.Tile, -_boost);
            }
        }

        //If there is no more entertainment in the group, remove it
        if (EntertainmentManager.Instance.GroupBoost[tile.GroupID].Count == 0)
        {
            EntertainmentManager.Instance.GroupBoost.Remove(tile.GroupID);
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
        }

        AddEntertainmentToGroup(targetGroupId, newEntertainment);
    }

    private void ResetEntertainmentPoints(Entertainment entertainment)
    {
        if (entertainment.Data != _dataBoosting)
            return;

        foreach (Entertainment ent in EntertainmentManager.Instance.GroupBoost[entertainment.Tile.GroupID])
        {
            if (ent == entertainment)
                continue;
            if (ent.Data != _dataBoosting)
                continue;
            entertainment.UpdatePoints(_boost, Transaction.Spent, false, ent.Tile);
        }
    }

    private bool CheckIfGroupStillWhole(int groupID)
    {
        var group = EntertainmentManager.Instance.GroupBoost[groupID];

        if (group.Count <= 1)
            return true;

        // BFS state: (current entertainment, lastWasBridge?)
        Queue<(Entertainment ent, bool lastWasBridge)> queue = new Queue<(Entertainment, bool)>();

        // visited separated by "previous was bridge" or "previous was not bridge"
        HashSet<Entertainment> visitedAfterBoost = new HashSet<Entertainment>();
        HashSet<Entertainment> visitedAfterBridge = new HashSet<Entertainment>();

        HashSet<Entertainment> reachable = new HashSet<Entertainment>();

        Entertainment start = group.First();
        bool startIsBridge = start.Data == _dataBridge;

        queue.Enqueue((start, startIsBridge));
        if (startIsBridge)
            visitedAfterBridge.Add(start);
        else
            visitedAfterBoost.Add(start);
        reachable.Add(start);

        while (queue.Count > 0)
        {
            var (current, lastWasBridge) = queue.Dequeue();

            foreach (Tile neighbor in current.Tile.Neighbors)
            {
                if (!neighbor)
                    continue;
                if (!neighbor.Entertainment)
                    continue;

                Entertainment nEnt = neighbor.Entertainment;
                if (!group.Contains(nEnt))
                    continue;

                bool isBridge = nEnt.Data == _dataBridge;
                bool isBoost = nEnt.Data == _dataBoosting;
                if (!isBridge && !isBoost)
                    continue;

                // Rule enforcement: forbid two bridges in a row
                if (isBridge && lastWasBridge)
                    continue;

                bool nextLastWasBridge = isBridge;
                HashSet<Entertainment> targetVisited = nextLastWasBridge ? visitedAfterBridge : visitedAfterBoost;

                if (!targetVisited.Add(nEnt))
                    continue; // already visited with this "previous" type

                queue.Enqueue((nEnt, nextLastWasBridge));
                reachable.Add(nEnt);
            }
        }

        return reachable.Count == group.Count;
    }

    private void SplitGroup(int groupID)
    {
        var group = EntertainmentManager.Instance.GroupBoost[groupID];

        // Tiles not yet assigned to a new component
        HashSet<Entertainment> remaining = new HashSet<Entertainment>(group);
        List<List<Entertainment>> splittedGroups = new List<List<Entertainment>>();

        while (remaining.Count > 0)
        {
            List<Entertainment> component = new List<Entertainment>();
            Queue<(Entertainment ent, bool lastWasBridge)> queue = new Queue<(Entertainment, bool)>();

            HashSet<Entertainment> visitedAfterBoost = new HashSet<Entertainment>();
            HashSet<Entertainment> visitedAfterBridge = new HashSet<Entertainment>();

            Entertainment start = remaining.First();
            bool startIsBridge = start.Data == _dataBridge;

            queue.Enqueue((start, startIsBridge));
            if (startIsBridge)
                visitedAfterBridge.Add(start);
            else
                visitedAfterBoost.Add(start);

            // We consider it part of this component
            component.Add(start);
            remaining.Remove(start);

            while (queue.Count > 0)
            {
                var (current, lastWasBridge) = queue.Dequeue();

                foreach (Tile neighbor in current.Tile.Neighbors)
                {
                    if (!neighbor)
                        continue;
                    if (!neighbor.Entertainment)
                        continue;

                    Entertainment nEnt = neighbor.Entertainment;

                    // Only pick tiles still unassigned to any other component
                    if (!remaining.Contains(nEnt))
                        continue;

                    bool isBridge = nEnt.Data == _dataBridge;
                    bool isBoost = nEnt.Data == _dataBoosting;
                    if (!isBridge && !isBoost)
                        continue;

                    // Rule enforcement: forbid two bridges in a row
                    if (isBridge && lastWasBridge)
                        continue;

                    bool nextLastWasBridge = isBridge;
                    HashSet<Entertainment> targetVisited = nextLastWasBridge ? visitedAfterBridge : visitedAfterBoost;

                    if (!targetVisited.Add(nEnt))
                        continue;

                    queue.Enqueue((nEnt, nextLastWasBridge));
                    component.Add(nEnt);
                    remaining.Remove(nEnt);
                }
            }

            splittedGroups.Add(component);
        }

        if (splittedGroups.Count > 1)
        {
            foreach (List<Entertainment> newGroup in splittedGroups)
            {
                bool isFirst = true;
                int newGroupID = 0;

                foreach (Entertainment ent in newGroup)
                {
                    ResetEntertainmentPoints(ent);
                    RemoveEntertainmentFromItsGroup(ent.Tile, ent.Data, false);

                    if (isFirst)
                    {
                        newGroupID = CreateNewGroup(ent);
                        isFirst = false;
                    }
                    else
                    {
                        // Skip VFX to avoid confusing the player
                        AddEntertainmentToGroup(newGroupID, ent, true);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("CheckIfGroupStillWhole shouldn't have returned false, groupID : " + groupID);
        }
    }


    public override string GetBehaviourDescription()
    {
        return $"Gain +{_boost}<sprite name=\"Point_Emoji\"> for each other {_dataBoosting.Type.ToCustomString()} connected to this one through a continuous zone. " +
               $"A {_dataBridge.Type.ToCustomString()} can bridge a one gap tile between two {_dataBoosting.Type.ToCustomString()}";
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SpecialEffect/BoostByZoneSize")]
public class BoostByZoneSize : SpecialEffect
{
    [SerializeField] private int _boost;
    [SerializeField] private EntertainmentData _dataBoosting;
    [SerializeField] private EntertainmentData _dataBridge;

    public override void InitializeSpecialEffect(Entertainment associatedEntertainment)
    {
        foreach (Tile neighbor in associatedEntertainment.Tile.Neighbors)
        {
            if (!neighbor)
                continue;
            neighbor.OnEntertainmentModified.RemoveListener(associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize);
            neighbor.OnEntertainmentModified.AddListener(associatedEntertainment.ListenerOnEntertainmentModified_BoostByZoneSize);
            if (!neighbor.Entertainment)
                continue;
            if (neighbor.GroupID > 0)
            {
                AddEntertainmentToGroup(neighbor.GroupID, associatedEntertainment);
                break;
            }  
        }

        //No group found, we create a new one
        if(associatedEntertainment.Tile.GroupID == 0)
            CreateNewGroup(associatedEntertainment);
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
                AddEntertainmentToGroup(associatedEntertainment.Tile.GroupID, updatedTile.Entertainment);
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
}

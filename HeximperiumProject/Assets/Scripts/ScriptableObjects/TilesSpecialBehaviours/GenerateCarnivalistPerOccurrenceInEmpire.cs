using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/GenerateCarnivalistPerOccurrenceInEmpire")]
public class GenerateCarnivalistPerOccurrenceInEmpire : SpecialBehaviour
{
    [SerializeField] private int _carnivalistQuantity;
    [SerializeField] private List<InfrastructureData> _tilesBoosting = new List<InfrastructureData>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
            {
                ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Gain, behaviourTile);
                ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Gain);
                behaviourTile.RecruitedCarnivalists += _carnivalistQuantity;
            }
        }
        ExploitationManager.Instance.OnInfraBuilded -= behaviourTile.ListenerOnInfraBuilded_GenerateCarnivalistPerOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraDestroyed -= behaviourTile.ListenerOnInfraDestroyed_GenerateCarnivalistPerOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraBuilded += behaviourTile.ListenerOnInfraBuilded_GenerateCarnivalistPerOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraDestroyed += behaviourTile.ListenerOnInfraDestroyed_GenerateCarnivalistPerOccurrenceInEmpire;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
            {
                ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Spent, behaviourTile);
                ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Spent);
                behaviourTile.RecruitedCarnivalists -= _carnivalistQuantity;
            }
        }

        ExploitationManager.Instance.OnInfraBuilded -= behaviourTile.ListenerOnInfraBuilded_GenerateCarnivalistPerOccurrenceInEmpire;
        ExploitationManager.Instance.OnInfraDestroyed -= behaviourTile.ListenerOnInfraDestroyed_GenerateCarnivalistPerOccurrenceInEmpire;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Tile tile in ExploitationManager.Instance.Infrastructures)
        {
            if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
            {
                tile.Highlight(show);
            }
        }
    }

    public void CheckNewInfra(Tile behaviourTile, Tile tile)
    {
        if (tile.TileData is InfrastructureData data && _tilesBoosting.Contains(data))
        {
            //Check if the previous data didn't already applied the boost
            if (tile.PreviousData is InfrastructureData previousData && _tilesBoosting.Contains(previousData))
                return;
            ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Gain, behaviourTile);
            ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Gain);
            behaviourTile.RecruitedCarnivalists += _carnivalistQuantity;
        }
        else
        {
            //Check if the previous data did apply a boost, then remove it if yes
            if (tile.PreviousData is InfrastructureData previousData && _tilesBoosting.Contains(previousData))
            {
                ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Spent, behaviourTile);
                ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Spent);
                behaviourTile.RecruitedCarnivalists -= _carnivalistQuantity;
            }
        }
    }

    public void CheckDestroyedInfra(Tile behaviourTile, Tile tile)
    {
        if (tile.PreviousData is InfrastructureData data && _tilesBoosting.Contains(data))
        {
            ResourcesManager.Instance.UpdateCarnivalist(_carnivalistQuantity, Transaction.Spent, behaviourTile);
            ResourcesManager.Instance.UpdateCarnivalistSource(behaviourTile.TileData, _carnivalistQuantity, Transaction.Spent);
            behaviourTile.RecruitedCarnivalists -= _carnivalistQuantity;
        }
    }

    public override string GetBehaviourDescription()
    {
        return $"Recruit {_carnivalistQuantity}<sprite name=\"Carnivalist_Emoji\"> for each occurrence of {_tilesBoosting.ToCustomString(true)} in the empire";
    }
}

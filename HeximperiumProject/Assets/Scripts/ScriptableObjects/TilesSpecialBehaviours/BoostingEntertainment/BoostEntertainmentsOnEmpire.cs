using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostEntertainmentsOnEmpire")]
public class BoostEntertainmentsOnEmpire : SpecialBehaviour
{
    [SerializeField] private int _boost;
    [SerializeField] private List<EntertainmentData> _entertainmentsBoosted = new List<EntertainmentData>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        EntertainmentManager.Instance.OnEntertainmentSpawned -= behaviourTile.ListenerOnEntertainmentSpawned;
        EntertainmentManager.Instance.OnEntertainmentSpawned += behaviourTile.ListenerOnEntertainmentSpawned;
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        EntertainmentManager.Instance.OnEntertainmentSpawned -= behaviourTile.ListenerOnEntertainmentSpawned;
    }

    public override void HighlightImpactedTile(Tile behaviourTile, bool show)
    {
        foreach (Entertainment ent in EntertainmentManager.Instance.Entertainments)
        {
            if (_entertainmentsBoosted.Contains(ent.Data))
                ent.Tile.Highlight(show);
        }
    }

    public void CheckEntertainment(Entertainment ent)
    {
        if (_entertainmentsBoosted.Contains(ent.Data))
            ent.UpdatePoints(_boost, Transaction.Gain);
        //No need to remove the boost, handled by Entertainment destruction method
    }

    public override string GetBehaviourDescription()
    {
        return $"Boosts every {_entertainmentsBoosted.ToCustomString()} in the empire by +{_boost}<sprite name=\"Point_Emoji\">";
    }
}

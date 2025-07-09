using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Special Behaviour/BoostEntertainmentsOnEmpire")]
public class BoostEntertainmentsOnEmpire : SpecialBehaviour
{
    [SerializeField] private int _boost;
    [SerializeField] private List<EntertainmentData> _entertainmentsBoosted = new List<EntertainmentData>();

    public override void InitializeSpecialBehaviour(Tile behaviourTile)
    {
        EntertainmentManager.Instance.OnEntertainmentSpawned.RemoveListener(behaviourTile.ListenerOnEntertainmentSpawned);
        EntertainmentManager.Instance.OnEntertainmentSpawned.AddListener(behaviourTile.ListenerOnEntertainmentSpawned);
    }

    public override void RollbackSpecialBehaviour(Tile behaviourTile)
    {
        EntertainmentManager.Instance.OnEntertainmentSpawned.RemoveListener(behaviourTile.ListenerOnEntertainmentSpawned);
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
}

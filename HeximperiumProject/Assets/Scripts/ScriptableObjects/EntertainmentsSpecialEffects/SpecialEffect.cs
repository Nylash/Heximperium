using UnityEngine;

public abstract class SpecialEffect : ScriptableObject
{
    public abstract void InitializeSpecialEffect(Entertainment associatedEntertainment);

    //Rollback the effect
    public abstract void RollbackSpecialEntertainment(Entertainment associatedEntertainment);

    //Method use to show entertainment impacted by the special effect
    public abstract void HighlightImpactedEntertainment(Entertainment associatedEntertainment, bool show);
}

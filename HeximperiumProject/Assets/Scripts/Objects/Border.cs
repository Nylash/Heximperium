using UnityEngine;

public class Border : MonoBehaviour
{
    /*
    Respect this order :
        Top-right
        Right
        Bottom-right
        Bottom-left
        Left
        Top-left
    */
    [SerializeField] private Animator[] _animators;

    public Tile associatedTile;

    public void CheckBorderVisibility()
    {
        for (int i = 0; i < associatedTile.Neighbors.Length; i++) 
        {
            if (!associatedTile.Neighbors[i])
                continue;
            //Neighbor is claimed, so no border fade out
            if(associatedTile.Neighbors[i].Claimed)
                _animators[i].SetTrigger("Fade");
        }
    }

    public void CheckPreviewBorderVisibility()
    {
        for (int i = 0; i < associatedTile.Neighbors.Length; i++)
        {
            if (!associatedTile.Neighbors[i])
                continue;
            //Neighbor is claimed, so no border fade out
            if (associatedTile.Neighbors[i].Claimed || ExpansionManager.Instance.InteractibleTiles.Contains(associatedTile.Neighbors[i]))
                _animators[i].SetTrigger("Fade");
        }
    }

    public void AnimationDone()
    {
        associatedTile.OnClaimBorderAnimationDone?.Invoke();
    }

    public void PreviewAnimationDone()
    {
        associatedTile.OnPreviewBorderAnimationDone?.Invoke();
    }
}

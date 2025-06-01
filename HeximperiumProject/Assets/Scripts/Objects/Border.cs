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
    [SerializeField] private Renderer[] _renderers;


    public void CheckBorderVisibility(Tile[] neighbors)
    {
        for (int i = 0; i < neighbors.Length; i++) 
        {
            //No tile so we activate the border
            if (neighbors[i] == null)
            {
                _renderers[i].enabled = true;
                continue;
            }
            //Neighbor is claimed, so no border
            if(neighbors[i].Claimed)
                _renderers[i].enabled = false;
            //Neighbor isn't claimed, so border
            else
                _renderers[i].enabled = true;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    //Return a list of world position around the tile, depending on how many buttons is needed
    public static List<Vector3> GetInteractionButtonsPosition(Vector3 tilePosition, int buttonsNumber)
    {
        List<Vector3> _positions = new List<Vector3>();
        switch (buttonsNumber)
        {
            case 1:
                _positions.Add(new Vector3(tilePosition.x, 0.5f, tilePosition.z + 1));
                return _positions;
            default:
                Debug.LogError("Interaction are not written for this many buttons : " + buttonsNumber);
                return _positions;
        }
    }
}

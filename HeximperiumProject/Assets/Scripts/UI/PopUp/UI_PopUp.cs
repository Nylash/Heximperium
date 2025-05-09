using UnityEngine;

public abstract class UI_PopUp : MonoBehaviour
{
    public abstract void InitializePopUp(Tile tile, UI_InteractionButton button = null); 
}

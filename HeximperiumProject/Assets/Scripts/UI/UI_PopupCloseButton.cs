using UnityEngine;
using UnityEngine.UI;

public class UI_PopupCloseButton : MonoBehaviour
{
    public void ClosePopup()
    {
        PopUpManager.Instance.CloseLockedPopup(GetComponent<Button>());
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PopUpMap
{
    private GameObject _popupObject;
    private Button _closeButton;

    public PopUpMap(GameObject popupObject, Button closeButton)
    {
        _popupObject = popupObject;
        _closeButton = closeButton;
    }

    public GameObject PopupObject { get => _popupObject; }
    public Button CloseButton { get => _closeButton; }
}

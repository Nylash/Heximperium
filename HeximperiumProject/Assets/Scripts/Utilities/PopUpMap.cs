using UnityEngine;
using UnityEngine.UI;

public class PopUpMap
{
    private GameObject _popupObject;
    private Button _closeButton;
    private GameObject _objectOrigin;

    public PopUpMap(GameObject popupObject, Button closeButton, GameObject objectOrigin)
    {
        _popupObject = popupObject;
        _closeButton = closeButton;
        _objectOrigin = objectOrigin;
    }

    public GameObject PopupObject { get => _popupObject; }
    public Button CloseButton { get => _closeButton; }
    public GameObject ObjectOrigin { get => _objectOrigin; }
}

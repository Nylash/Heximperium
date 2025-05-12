using UnityEngine;

public class PopUpUI_Basic : UI_PopUp
{
    public override GameObject InitializePopUp(Transform canvas)
    {
        return Instantiate(_popUp, canvas);
    }
}
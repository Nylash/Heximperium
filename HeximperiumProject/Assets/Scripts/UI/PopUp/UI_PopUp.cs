using UnityEngine;

public abstract class UI_PopUp : MonoBehaviour
{
    [SerializeField] protected GameObject _popUp;

    public abstract GameObject InitializePopUp(Transform canvas);
}

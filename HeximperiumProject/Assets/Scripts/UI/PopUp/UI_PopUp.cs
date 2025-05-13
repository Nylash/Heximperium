using UnityEngine;

public class UI_PopUp : MonoBehaviour
{
    [SerializeField] protected GameObject _popUp;

    public virtual GameObject SpawnPopUp(Transform canvas)
    {
        return Instantiate(_popUp, canvas);
    }
}

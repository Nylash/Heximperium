using UnityEngine;

public class SpawnUIPopUp : MonoBehaviour
{
    [SerializeField] protected GameObject _popUp;

    public virtual GameObject SpawnPopUp(Transform canvas)
    {
        return Instantiate(_popUp, canvas);
    }
}

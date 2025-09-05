using UnityEngine;

public class SetRendererSortingOrder : MonoBehaviour
{
    [SerializeField] private int sortingOrder;

    private void Start()
    {
        GetComponent<Renderer>().sortingOrder = sortingOrder;
    }
}

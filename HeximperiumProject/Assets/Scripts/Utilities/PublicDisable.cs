using UnityEngine;

public class PublicDisable : MonoBehaviour
{
    public void DisableThis()
    {
        gameObject.SetActive(false);
    }
}

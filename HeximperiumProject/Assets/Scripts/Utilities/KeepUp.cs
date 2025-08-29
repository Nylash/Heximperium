using UnityEngine;

public class KeepUp : MonoBehaviour
{
    void LateUpdate()
    {
        transform.up = Vector3.up;
    }
}

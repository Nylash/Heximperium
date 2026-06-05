using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    [SerializeField] Vector2 _size;
    [SerializeField] private float _gizmoHeight = 0.5f;

    public Vector3 ClampPosition(Vector3 position)
    {
        Vector3 center = transform.position;
        Vector2 halfSize = _size * 0.5f;

        position.x = Mathf.Clamp(position.x, center.x - halfSize.x, center.x + halfSize.x);
        position.z = Mathf.Clamp(position.z, center.z - halfSize.y, center.z + halfSize.y);

        return position;
    }

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position + Vector3.up * _gizmoHeight;
        Vector2 halfSize = _size * 0.5f;

        Vector3 bottomLeft = center + new Vector3(-halfSize.x, 0, -halfSize.y);
        Vector3 bottomRight = center + new Vector3(halfSize.x, 0, -halfSize.y);
        Vector3 topRight = center + new Vector3(halfSize.x, 0, halfSize.y);
        Vector3 topLeft = center + new Vector3(-halfSize.x, 0, halfSize.y);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}

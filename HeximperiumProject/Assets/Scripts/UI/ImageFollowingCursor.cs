using UnityEngine;

public class ImageFollowingCursor : MonoBehaviour
{
    [SerializeField] private RectTransform _imageRectTransform;
    [SerializeField] private Vector2 _offset = new Vector2(20, 20);

    private void Update()
    {
        _imageRectTransform.position = Input.mousePosition + (Vector3)_offset;
    }
}

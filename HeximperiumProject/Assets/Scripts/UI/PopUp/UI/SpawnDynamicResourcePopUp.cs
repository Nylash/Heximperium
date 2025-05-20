using UnityEngine;

public class SpawnDynamicResourcePopUp : SpawnUIPopUp
{
    [SerializeField] private Resource _resource;
    [SerializeField][Range(0.0f,1.0f)] private float _anchorMinX;
    [SerializeField][Range(0.0f, 1.0f)] private float _anchorMaxX;

    public override GameObject SpawnPopUp(Transform canvas)
    {
        GameObject popUp = Instantiate(_popUp, canvas);

        RectTransform rectTransform = popUp.GetComponent<RectTransform>();

        // Get the current anchor values
        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;

        // Modify only the X values of the anchors
        anchorMin.x = _anchorMinX;
        anchorMax.x = _anchorMaxX;

        // Set the modified anchor values
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Set the new offsets to match the new anchors
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        popUp.GetComponent<PopUp_AdvancedResources>().Resource = _resource;

        return popUp;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PopUpManager : Singleton<PopUpManager>
{
    [Header("_________________________________________________________")]
    [Header("PopUp UI")]
    [SerializeField] private float _durationHoverForUI = 1f;
    [SerializeField] private float _offsetBetweenSeveralPopUps = 1f;
    [SerializeField] private float _minOffset = 80f;
    [SerializeField] private float _maxOffset = 275f;

    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _screenWidth;
    private float _screenHeight;
    private List<GameObject> _popUps = new List<GameObject>();

    public void UIPopUp(GameObject obj)
    {
        if (obj == _objectUnderMouse)
        {
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                //spawn UI pop up
                //_popUps.Add(popUp);
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUp(obj);
        }
    }

    public void NonUIPopUp(GameObject obj)
    {
        if (obj == _objectUnderMouse)
        {
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popUps.Count == 0)
            {
                if (obj.GetComponent<Tile>() is Tile tile)
                {
                    if (tile.Revealed)
                    {
                        //Popup tile
                        if (tile.Scouts.Count > 0)
                        {
                            foreach (Scout item in tile.Scouts)
                            {
                                //Popup scout
                            }
                        }
                        //if (tile.Entertainment != null)
                            //Popup entertainment
                    }
                }
                else if (obj.GetComponent<InteractionButton>() is InteractionButton button)
                {
                    //Popup interaction button
                }
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            ResetPopUp(obj);
        }
    }

    public void ResetPopUp(GameObject obj)
    {
        _objectUnderMouse = obj;
        _hoverTimer = 0.0f;

        if (_popUps.Count > 0)
        {
            foreach (GameObject item in _popUps)
            {
                //Destrou popup
            }
            _popUps.Clear();
        }
    }

    private void DisplayPopUp<T>(T item, GameObject prefab)
    {
        GameObject popUp;

        // Spawn the pop up
        popUp = Instantiate(prefab, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        // Initialize the popup

        // Position the pop up relatively to the mouse cursor
        PositionPopup(popUp.transform, GetPopUpSize(popUp.GetComponent<RectTransform>()));
    }

    private void PositionPopup(Transform popup, Vector2 popupSize)
    {
        // Get the current mouse position
        Vector3 mousePosition = Input.mousePosition;

        // Determine the quadrant
        bool isLeft = mousePosition.x < (_screenWidth / 2);
        bool isTop = mousePosition.y > (_screenHeight / 2);

        // Calculate the cumulative vertical offset based on the sizes of previous pop-ups
        float verticalOffset = 0;
        //_popUps.Count -1 to avoid counting the current pop up
        for (int i = 0; i < _popUps.Count - 1; i++)
        {
            RectTransform previousPopupRectTransform = _popUps[i].GetComponent<RectTransform>();
            verticalOffset += previousPopupRectTransform.rect.height + _offsetBetweenSeveralPopUps;
        }

        // Get the current zoom level from the camera
        float zoomLevel = CameraManager.Instance.transform.position.y;

        // Calculate the dynamic offset based on the zoom level
        float normalizedZoom = Mathf.InverseLerp(CameraManager.Instance.MinZoomLevel, CameraManager.Instance.MaxZoomLevel, zoomLevel);
        float dynamicOffset = Mathf.Lerp(_minOffset, _maxOffset, normalizedZoom);

#if UNITY_EDITOR
        dynamicOffset = 0;//Remove the offset in the editor (game screen is small so popup aren't visible)
#endif

        if (isLeft && isTop)
        {
            // Top-Left Quadrant: Snap top-left corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x + popupSize.x / 2 + dynamicOffset,
                mousePosition.y - popupSize.y / 2 - verticalOffset - dynamicOffset,
                0);
        }
        else if (!isLeft && isTop)
        {
            // Top-Right Quadrant: Snap top-right corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x - popupSize.x / 2 - dynamicOffset,
                mousePosition.y - popupSize.y / 2 - verticalOffset - dynamicOffset,
                0);
        }
        else if (isLeft && !isTop)
        {
            // Bottom-Left Quadrant: Snap bottom-left corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x + popupSize.x / 2 + dynamicOffset,
                mousePosition.y + popupSize.y / 2 + verticalOffset + dynamicOffset,
                0);
        }
        else
        {
            // Bottom-Right Quadrant: Snap bottom-right corner to cursor with dynamic offset
            popup.position = new Vector3(
                mousePosition.x - popupSize.x / 2 - dynamicOffset,
                mousePosition.y + popupSize.y / 2 + verticalOffset + dynamicOffset,
                0);
        }
    }

    private Vector2 GetPopUpSize(RectTransform rectTransform)
    {
        return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
    }
}

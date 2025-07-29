using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : Singleton<PopUpManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Spawning Configuration")]
    [SerializeField] private float _durationHoverForUI = 1f;
    [SerializeField] private float _offsetBetweenSeveralPopUps = 1f;
    [SerializeField] private float _minOffset = 80f;
    [SerializeField] private float _maxOffset = 275f;
    [SerializeField] private float _maxScreenFraction = 0.15f;
    [SerializeField] private Vector4 _margin = new Vector4(10, 0, 10, 0); // left, top, right, bottom
    [Header("_________________________________________________________")]
    [Header("Prefabs")]
    [SerializeField] private GameObject _basePopUp;
    [SerializeField] private GameObject _title;
    [SerializeField] private GameObject _text;
    #endregion

    #region VARIABLES
    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _screenWidth;
    private float _screenHeight;
    private List<GameObject> _popUps = new List<GameObject>();
    float _maxAllowed;
    #endregion

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        _maxAllowed = _screenWidth * _maxScreenFraction;
    }

    #region BASE LOGIC
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
                        TilePopUp(tile);
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
                    Destroy(item);
            }
            _popUps.Clear();
        }
    }
    #endregion

    private void TilePopUp(Tile tile)
    {
        GameObject popUp;
        popUp = Instantiate(_basePopUp, UIManager.Instance.PopUpParent);
        _popUps.Add(popUp);

        List<RectTransform> textObjects = new List<RectTransform>();

        TextMeshProUGUI title = Instantiate(_title, popUp.transform).GetComponent<TextMeshProUGUI>();
        title.text = tile.TileData.TileName;
        textObjects.Add(title.GetComponent<RectTransform>());

        TextMeshProUGUI details = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        //Special behaviour
        //details.text = tile.TileData.TilePopUpText;
        details.margin = _margin;
        textObjects.Add(details.GetComponent<RectTransform>());
        ClampTextWidth(details);//Manage the width of the longest text
        details.fontStyle = FontStyles.Italic;

        if (tile.Incomes.Count > 0)
        {
            TextMeshProUGUI income = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
            income.text = tile.Incomes.ToCustomString();
            income.margin = _margin;
            textObjects.Add(income.GetComponent<RectTransform>());
        }

        TextMeshProUGUI claimStatus = Instantiate(_text, popUp.transform).GetComponent<TextMeshProUGUI>();
        if (tile.Claimed)
        {
            claimStatus.text = "Tile claimed";
            claimStatus.fontStyle = FontStyles.Italic;
            claimStatus.alignment = TextAlignmentOptions.Right;
        }
        else
            claimStatus.text = "Claim cost: " + tile.TileData.ClaimCost + "<sprite name=\"Claim_Emoji\">";
        claimStatus.margin = _margin;
        textObjects.Add(claimStatus.GetComponent<RectTransform>());

        SetPopUpContentAnchors(textObjects);

        PositionPopup(popUp.GetComponent<RectTransform>());
    }

    #region POSITIONING & LAYOUT
    private void PositionPopup(RectTransform popupRect)
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

        Vector2 pivot;
        Vector3 anchoredPos = mousePosition;

        if (isLeft && isTop)
        {
            pivot = new Vector2(0f, 1f); // Top-left
            anchoredPos += new Vector3(dynamicOffset, -dynamicOffset - verticalOffset, 0);
        }
        else if (!isLeft && isTop)
        {
            pivot = new Vector2(1f, 1f); // Top-right
            anchoredPos += new Vector3(-dynamicOffset, -dynamicOffset - verticalOffset, 0);
        }
        else if (isLeft && !isTop)
        {
            pivot = new Vector2(0f, 0f); // Bottom-left
            anchoredPos += new Vector3(dynamicOffset, dynamicOffset + verticalOffset, 0);
        }
        else
        {
            pivot = new Vector2(1f, 0f); // Bottom-right
            anchoredPos += new Vector3(-dynamicOffset, dynamicOffset + verticalOffset, 0);
        }

        // Apply pivot and position
        popupRect.pivot = pivot;
        popupRect.position = anchoredPos;
    }

    private void ClampTextWidth(TextMeshProUGUI tmp)
    {
        tmp.ForceMeshUpdate();
        float contentWidth = tmp.preferredWidth;
        tmp.GetComponent<LayoutElement>().preferredWidth = Mathf.Min(contentWidth, _maxAllowed);
    }

    private void SetPopUpContentAnchors(List<RectTransform> textObjects)
    {
        int count = textObjects.Count;
        for (int i = 0; i < count; i++)
        {
            int reversedIndex = count - 1 - i;
            textObjects[reversedIndex].anchorMin = new Vector2(0, i / (float)count);
            textObjects[reversedIndex].anchorMax = new Vector2(1, (i + 1f) / count);
        }
    }
    #endregion
}

using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] Transform _mainCanvas;
    [Header("Resources")]
    [SerializeField] TextMeshProUGUI _claimText;
    [Header("Phase UI")]
    [SerializeField] TextMeshProUGUI _currentPhaseText;
    [SerializeField] TextMeshProUGUI _confirmPhaseButtonText;
    [SerializeField] TextMeshProUGUI _turnCounterText;
    [Header("PopUp UI")]
    [SerializeField] GameObject _unclaimedTilePopup;
    [SerializeField] float _durationHoverForUI = 2.0f;

    private GameObject _objectUnderMouse;
    private float _hoverTimer;
    private float _screenWidth;
    private float _screenHeight;
    private GameObject _popup;

    protected override void OnAwake()
    {
        GameManager.Instance.event_newTurn.AddListener(UpdateTurnCounterText);
        GameManager.Instance.event_newPhase.AddListener(UpdatePhaseUI);
    }

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
}

    public void UpdateResourceUI(Resource resource, int value)
    {
        switch (resource)
        {
            case Resource.Stone:
                break;
            case Resource.Essence:
                break;
            case Resource.Horse:
                break;
            case Resource.Pigment:
                break;
            case Resource.Crystal:
                break;
            case Resource.Emberbone:
                break;
            case Resource.Claim:
                _claimText.text = value.ToString();
                break;
            case Resource.Point:
                break;
        }
    }

    #region PopUp UI
    public void HoverUIPopupCheck(GameObject obj)
    {
        if (obj == _objectUnderMouse) 
        {
            //Timer before spawning popup
            _hoverTimer += Time.deltaTime;
            if (_hoverTimer >= _durationHoverForUI && _popup == null) 
            {
                DisplayPopUp(obj.GetComponent<Tile>());
            }
        }
        else
        {
            //Object under cursor changed, so we reset everything
            _objectUnderMouse = obj;
            _hoverTimer = 0.0f;
            Destroy(_popup);
        }
    }

    private void DisplayPopUp(Tile tile)
    {
        //if (!tile.Revealed)
          //  return;
        if (!tile.Claimed)
        {
            TileData data = tile.TileData;
            _popup = Instantiate(_unclaimedTilePopup, _mainCanvas);
            _popup.GetComponent<UI_UnclaimedTile>().Initialize(data.TileName, tile.Biome.ToString(), data.TextEffect, data.GoldIncome.ToString(), data.ClaimCost.ToString());

            PositionPopup(_popup.transform, GetPopUpSize(_popup.GetComponent<RectTransform>()));

            _popup.SetActive(true);
        }
    }

    private void PositionPopup(Transform popup, Vector2 popupSize)
    {
        // Get the current mouse position
        Vector3 mousePosition = Input.mousePosition;

        // Determine the quadrant
        bool isLeft = mousePosition.x < (_screenWidth / 2);
        bool isTop = mousePosition.y > (_screenHeight / 2);

        if (isLeft && isTop)
        {
            // Top-Left Quadrant: Snap top-left corner to cursor
            popup.position = new Vector3(mousePosition.x + popupSize.x / 2, mousePosition.y - popupSize.y / 2, 0);
        }
        else if (!isLeft && isTop)
        {
            // Top-Right Quadrant: Snap top-right corner to cursor
            popup.position = new Vector3(mousePosition.x - popupSize.x / 2, mousePosition.y - popupSize.y / 2, 0);
        }
        else if (isLeft && !isTop)
        {
            // Bottom-Left Quadrant: Snap bottom-left corner to cursor
            popup.position = new Vector3(mousePosition.x + popupSize.x / 2, mousePosition.y + popupSize.y / 2, 0);
        }
        else
        {
            // Bottom-Right Quadrant: Snap bottom-right corner to cursor
            popup.position = new Vector3(mousePosition.x - popupSize.x / 2, mousePosition.y + popupSize.y / 2, 0);
        }

    }

    private Vector2 GetPopUpSize(RectTransform rectTransform)
    {
        return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
    }
#endregion

    #region Phase UI
    private void UpdatePhaseUI(Phase phase)
    {
        switch (phase)
        {
            case Phase.Explore:
                _currentPhaseText.text = "Explore";
                _confirmPhaseButtonText.text = "End Phase";
                break;
            case Phase.Expand:
                _currentPhaseText.text = "Expand";
                _confirmPhaseButtonText.text = "End Phase";
                break;
            case Phase.Exploit:
                _currentPhaseText.text = "Exploit";
                _confirmPhaseButtonText.text = "End Phase";
                break;
            case Phase.Entertain:
                _currentPhaseText.text = "Entertain";
                _confirmPhaseButtonText.text = "End Turn";
                break;
        }
    }

    private void UpdateTurnCounterText(int turnCounter)
    {
        _turnCounterText.text = "Turn : " + turnCounter;
    }
    #endregion
}

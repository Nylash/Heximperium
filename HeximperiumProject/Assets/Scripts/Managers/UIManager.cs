using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] Transform _mainCanvas;
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

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
}

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

    public void UpdatePhaseText()
    {
        switch (GameManager.Instance.CurrentPhase)
        {
            case Phase.Explore:
                _currentPhaseText.text = "Explore";
                break;
            case Phase.Expand:
                _currentPhaseText.text = "Expand";
                break;
            case Phase.Exploit:
                _currentPhaseText.text = "Exploit";
                break;
            case Phase.Entertain:
                _currentPhaseText.text = "Entertain";
                break;
            default:
                Debug.LogError("No matching phase.");
                break;
        }
    }

    public void UpdatePhaseButtonText() 
    {
        if (GameManager.Instance.CurrentPhase == Phase.Entertain)
        {
            _confirmPhaseButtonText.text = "End Turn";
        }
        else
        {
            _confirmPhaseButtonText.text = "End Phase";
        }
    }

    public void UpdateTurnCounterText()
    {
        _turnCounterText.text = "Turn : " + GameManager.Instance.TurnCounter;
    }
}

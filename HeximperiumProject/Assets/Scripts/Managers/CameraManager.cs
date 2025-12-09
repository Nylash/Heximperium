using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraManager : Singleton<CameraManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("Camera Movement Settings")]
    [SerializeField] private float _cameraMovementSpeed = 5;
    [SerializeField] private float _cameraDragSpeed = 2;
    [SerializeField] private float _moveEventThreshold = 2f;
    [Header("_________________________________________________________")]
    [Header("Edge Pan Settings")]
#pragma warning disable CS0414
    [SerializeField] private float _edgePanMargin = 2;
    [SerializeField] private float _edgePanSpeed = 3;
#pragma warning restore CS0414
    [Header("_________________________________________________________")]
    [Header("Zoom Settings")]
    [SerializeField] private float _cameraZoomSpeed = 10;
    [SerializeField] private float _maxZoomLevel = 20.0f; //Far
    [SerializeField] private float _minZoomLevel = 3.5f; //Close
    [SerializeField] private float _zoomEventThreshold = 1f;
    #endregion

    #region VARIABLES
    private InputSystem_Actions _inputActions;
    //Camera
    private Vector2 _cameraMovement;
    private float _cameraZoom;
    private Vector3 _initialPos;
    //Drag
    private bool _isMouseDragging;
    private Vector2 _lastMousePosition;
    //Edge pan
    private Vector2 _mousePosition;
    private Vector2 _direction;
    //Mouse over
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private Ray _mouseRay;
    private RaycastHit _mouseRayHit;
    private InteractionButton _shrinkedButton;
    //Tutorial variables
    private Vector2 _lastPositionEventXZ;
    private float _lastZoomEventY;
    #endregion

    #region EVENTS
    //Tutorial events
    public event Action OnCameraMoved;
    public event Action OnCameraZoomed;
    public event Action OnCameraCentered;
    #endregion

    private void OnEnable() => _inputActions.Player.Enable();
    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
        _initialPos = transform.position;

        _inputActions = new InputSystem_Actions();

        //Key input
        _inputActions.Player.CameraMovement.performed += ctx => _cameraMovement = ctx.ReadValue<Vector2>().normalized;
        _inputActions.Player.CameraMovement.canceled += ctx => _cameraMovement = Vector2.zero;

        //Zoom input
        _inputActions.Player.CameraZoom.performed += ctx => _cameraZoom = -ctx.ReadValue<Vector2>().y;
        _inputActions.Player.CameraZoom.canceled += ctx => _cameraZoom = 0.0f;

        //Drag input
        _inputActions.Player.RightClick.started += ctx => StartDragging();
        _inputActions.Player.RightClick.canceled += ctx => _isMouseDragging = false;
        _inputActions.Player.MouseMovement.performed += ctx => DragCamera();

        _inputActions.Player.CenterCam.performed += ctx => CenterCam();
    }

    private void Start()
    {
        // only look in the scene this camera lives in:
        raycaster = gameObject.scene
          .GetRootGameObjects()
          .SelectMany(go => go.GetComponentsInChildren<GraphicRaycaster>())
          .FirstOrDefault();

        eventSystem = EventSystem.current;

        _lastPositionEventXZ = new Vector2(transform.position.x, transform.position.z);
        _lastZoomEventY = transform.position.y;
    }

    private void Update()
    {
        if (GameManager.Instance.GamePaused)
        {
            if (UIManager.Instance.UpgradesChoiceMenuObject.activeSelf)
                ObjectUnderMouseDetection();
            return;
        }

        if (!_isMouseDragging)
        {
            KeyMovement();
            EdgePan();
        }
        Zoom();

        if (TutorialManager.Instance != null)
        {
            Vector3 pos = transform.position;
            Vector2 currentXZ = new Vector2(pos.x, pos.z);

            Vector2 delta = currentXZ - _lastPositionEventXZ;

            // Fire event only if we've moved enough since last event
            if (delta.sqrMagnitude >= _moveEventThreshold * _moveEventThreshold)
            {
                _lastPositionEventXZ = currentXZ;
                OnCameraMoved?.Invoke();
            }
        }

        if (ExplorationManager.Instance.ChoosingScoutDirection)
        {
            PopUpManager.Instance.ResetPopUp(null);
            return;
        }   

        ObjectUnderMouseDetection();
    }

    #region MOUSE OVER DETECTION
    //Check if the cursor is over an object, if so give the object to UI Manager to display a pop up
    private void ObjectUnderMouseDetection()
    {
        // Check if the pointer is over a UI GameObject
        if (EventSystem.current.IsPointerOverGameObject())
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {
                TextMeshProUGUI text = results[0].gameObject.GetComponent<TextMeshProUGUI>();
                if (text != null && results[0].gameObject.CompareTag("Untagged"))
                {
                    DetectWordUnderCursor(pointerEventData, text);
                }
                else
                {
                    // Pass the topmost UI object under the cursor
                    PopUpManager.Instance.UIPopUp(results[0].gameObject);
                }
            }

            //If a interaction button was shrink we unshrink it
            if (_shrinkedButton != null)
            {
                _shrinkedButton.ShrinkAnimation(false);
                _shrinkedButton = null;
            }
        }
        else
        {
            _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_mouseRay, out _mouseRayHit))
            {
                PopUpManager.Instance.NonUIPopUp(_mouseRayHit.collider.gameObject);

                //If we detect a InteractionButton we play the shrink animation
                if (_mouseRayHit.collider.gameObject.GetComponent<InteractionButton>() is InteractionButton button)
                {
                    //Check if the cursor is over a new InteractionButton and so unshrink the previous one (if there is one)
                    if (_shrinkedButton != null)
                    {
                        if (_shrinkedButton != button)
                        {
                            _shrinkedButton.ShrinkAnimation(false);
                            _shrinkedButton = null;
                        }
                    }
                    button.ShrinkAnimation(true);
                    _shrinkedButton = button;
                }
                else
                {
                    //If a interaction button was shrink we unshrink it
                    if (_shrinkedButton != null)
                    {
                        _shrinkedButton.ShrinkAnimation(false);
                        _shrinkedButton = null;
                    }
                }
            }
        }
    }

    private void DetectWordUnderCursor(PointerEventData eventData, TextMeshProUGUI text)
    {
        text.ForceMeshUpdate();

        int w = TMP_TextUtilities.FindIntersectingWord(text, eventData.position, eventData.enterEventCamera);
        if (w == -1) return;

        var wi = text.textInfo.wordInfo[w];
        string s = wi.GetWord();                 // raw word (no tags)

        if (!Utilities.IsUnderlined(text, wi.firstCharacterIndex, wi.lastCharacterIndex))
            return;

        string underlinedWord = Utilities.ExtractWholeUnderlinedWord(text.textInfo, wi.firstCharacterIndex);

        foreach (Family family in Enum.GetValues(typeof(Family)))
        {
            if (Utilities.Matches(underlinedWord, family.ToString(), true))
            {
                PopUpManager.Instance.PopUpOnPopUp(text.rectTransform, family);
                return;
            }
        }
        foreach (InfrastructureData infra in ExploitationManager.Instance.AllInfraDatas)
        {
            if (underlinedWord.Equals(infra.TileName))
            {
                PopUpManager.Instance.PopUpOnPopUp(text.rectTransform, Family.None, infra);
                return;
            }
        }
        foreach (EntertainmentData ent in EntertainmentManager.Instance.EntertainmentsData)
        {
            string entName = ent.Type.ToCustomString(false);
            if (Utilities.Matches(underlinedWord, entName, false))
            {
                PopUpManager.Instance.PopUpOnPopUp(text.rectTransform, Family.None, null, ent);
                return;
            }
        }
    }
    #endregion

    #region CAMERA MOVEMENT
    private void CenterCam()
    {
        if (GameManager.Instance.GamePaused)
            return;

        transform.position = _initialPos;

        OnCameraCentered?.Invoke();
    }

    private void Zoom()
    {
        var pos = transform.position;

        float currentY = pos.y;
        float targetY = Mathf.Clamp(
            currentY + _cameraZoom,
            _maxZoomLevel,
            _minZoomLevel
        );

        float newY = Mathf.Lerp(
            currentY,
            targetY,
            _cameraZoomSpeed * Time.deltaTime
        );

        pos.y = newY;
        transform.position = pos;

        // Fire event only if we've moved enough since last event
        if (Mathf.Abs(newY - _lastZoomEventY) >= _zoomEventThreshold)
        {
            _lastZoomEventY = newY;
            OnCameraZoomed?.Invoke();
        }
    }

    private void MoveCamera(Vector2 direction, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position,
            transform.position + new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime, 0.5f);
    }

    private void KeyMovement()
    {
        MoveCamera(_cameraMovement, _cameraMovementSpeed);
    }

    private void DragCamera()
    {
        if (GameManager.Instance.GamePaused)
            return;

        if (_isMouseDragging)
        {
            Vector2 delta = Mouse.current.position.ReadValue() - _lastMousePosition;
            MoveCamera(new Vector2(-delta.x, -delta.y), _cameraDragSpeed);
            _lastMousePosition = Mouse.current.position.ReadValue();
        }
    }

    private void StartDragging()
    {
        if (GameManager.Instance.GamePaused)
            return;

        _isMouseDragging = true;
        _lastMousePosition = Mouse.current.position.ReadValue();
    }

    private void EdgePan()
    {
        //Disable the behaviour in the editor to avoid annoying behaviour
        #if UNITY_EDITOR

        #else

        _mousePosition = Mouse.current.position.ReadValue();
        _direction = Vector2.zero;

        if (_mousePosition.x < _edgePanMargin)
        {
            _direction.x = -1;
        }
        else if (_mousePosition.x > Screen.width - _edgePanMargin)
        {
            _direction.x = 1;
        }

        if (_mousePosition.y < _edgePanMargin)
        {
            _direction.y = -1;
        }
        else if (_mousePosition.y > Screen.height - _edgePanMargin)
        {
            _direction.y = 1;
        }

        if (_direction != Vector2.zero)
        {
            transform.position += new Vector3(_direction.x, 0, _direction.y) * _edgePanSpeed * Time.deltaTime;
        }
        #endif
    }

    //Draw Edge pan margin
    /*private void OnGUI()
    {
        // Draw the edge pan margins
        GUI.color = new Color(1, 0, 0, 0.5f); // Semi-transparent red

        // Top margin
        GUI.DrawTexture(new Rect(0, 0, Screen.width, _edgePanMargin), Texture2D.whiteTexture);
        // Bottom margin
        GUI.DrawTexture(new Rect(0, Screen.height - _edgePanMargin, Screen.width, _edgePanMargin), Texture2D.whiteTexture);
        // Left margin
        GUI.DrawTexture(new Rect(0, 0, _edgePanMargin, Screen.height), Texture2D.whiteTexture);
        // Right margin
        GUI.DrawTexture(new Rect(Screen.width - _edgePanMargin, 0, _edgePanMargin, Screen.height), Texture2D.whiteTexture);
    }*/
#endregion
}

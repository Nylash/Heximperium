using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraManager : Singleton<CameraManager>
{
    #region CONSTANTS
    private const float MIN_ZOOM_LEVEL = 2.0f;
    private const float MAX_ZOOM_LEVEL = 20.0f;
    #endregion

    #region CONFIGURATION
    [SerializeField] private float _cameraMovementSpeed = 5;
    [SerializeField] private float _cameraDragSpeed = 2;
    [SerializeField] private float _cameraZoomSpeed = 10;
#pragma warning disable CS0414
    [SerializeField] private float _edgePanMargin = 2;
    [SerializeField] private float _edgePanSpeed = 3;
#pragma warning restore CS0414
    #endregion

    #region VARIABLES
    private InputSystem_Actions _inputActions;
    //Camera
    private Vector2 _cameraMovement;
    private float _cameraZoom;
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
    private UI_InteractionButton _shrinkedButton;
    #endregion

    private void OnEnable() => _inputActions.Player.Enable();
    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
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
    }

    private void Start()
    {
        raycaster = FindFirstObjectByType<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (!_isMouseDragging)
        {
            KeyMovement();
            EdgePan();
        }
        Zoom();

        ObjectUnderMouseDetection();
    }

    //Check if the cursor is over an object, if so give the object to UI Manager to display a pop up
    private void ObjectUnderMouseDetection()
    {
        // Check if the pointer is over a UI GameObject
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("UI");
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {
                // Pass the topmost UI object under the cursor
                UIManager.Instance.PopUpUI(results[0].gameObject);
            }

            //If a interaction button was shrink we unshrink it
            if (_shrinkedButton != null)
            {
                Debug.Log("Unshrinking button." + _shrinkedButton);
                _shrinkedButton.ShrinkAnimation(false);
                _shrinkedButton = null;
            }
        }
        else
        {
            _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_mouseRay, out _mouseRayHit))
            {
                UIManager.Instance.PopUpNonUI(_mouseRayHit.collider.gameObject);

                //If we detect a InteractionButton we play the shrink animation
                if (_mouseRayHit.collider.gameObject.GetComponent<UI_InteractionButton>() is UI_InteractionButton button)
                {
                    Debug.Log("Hit a button.");

                    //Check if the cursor is over a new InteractionButton and so unshrink the previous one (if there is one)
                    if (_shrinkedButton != null)
                    {
                        Debug.Log("Got a previous button");
                        if (_shrinkedButton != button)
                        {
                            Debug.Log("Unshrinking button." + _shrinkedButton);
                            _shrinkedButton.ShrinkAnimation(false);
                            _shrinkedButton = null;
                        }
                    }

                    Debug.Log("Shrink button." + button);
                    button.ShrinkAnimation(true);
                    _shrinkedButton = button;
                }
                else
                {
                    Debug.Log("Hit something else than a button");

                    //If a interaction button was shrink we unshrink it
                    if (_shrinkedButton != null)
                    {
                        Debug.Log("Unshrinking button." + _shrinkedButton);
                        _shrinkedButton.ShrinkAnimation(false);
                        _shrinkedButton = null;
                    }
                }
            }
            else
                Debug.Log("NOTHING ?");
        }
    }

    #region CAMERA MOVEMENT
    private void Zoom()
    {
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Clamp(Mathf.Lerp(transform.position.y, transform.position.y + _cameraZoom, _cameraZoomSpeed * Time.deltaTime), MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL),
            transform.position.z
            );
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
        if (_isMouseDragging)
        {
            Vector2 delta = Mouse.current.position.ReadValue() - _lastMousePosition;
            MoveCamera(new Vector2(-delta.x, -delta.y), _cameraDragSpeed);
            _lastMousePosition = Mouse.current.position.ReadValue();
        }
    }

    private void StartDragging()
    {
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

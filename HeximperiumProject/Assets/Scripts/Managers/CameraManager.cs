using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private float _cameraMovementSpeed = 3;
    [SerializeField] private float _cameraDragSpeed = 1;
    [SerializeField] private float _cameraZoomSpeed = 7;
#pragma warning disable CS0414
    [SerializeField] private float _edgePanMargin = 20;
    [SerializeField] private float _edgePanSpeed = 5;
#pragma warning restore CS0414

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

    private void Update()
    {
        if (!_isMouseDragging)
        {
            KeyMovement();
            EdgePan();
        }
        Zoom();
    }

    private void KeyMovement()
    {
        transform.position = Vector3.MoveTowards(transform.position,
            transform.position + new Vector3(_cameraMovement.x, 0, _cameraMovement.y) * _cameraMovementSpeed * Time.deltaTime,
            0.5f);
    }

    private void Zoom()
    {
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Clamp(Mathf.Lerp(transform.position.y, transform.position.y + _cameraZoom, _cameraZoomSpeed * Time.deltaTime), 2.0f, 20.0f),
            transform.position.z
            );
    }

    private void StartDragging()
    {
        _isMouseDragging = true;
        _lastMousePosition = Mouse.current.position.ReadValue();
    }

    private void DragCamera()
    {
        if (_isMouseDragging)
        {
            Vector2 delta = Mouse.current.position.ReadValue() - _lastMousePosition;
            transform.position = Vector3.MoveTowards(transform.position,
                transform.position + new Vector3(-delta.x, 0, -delta.y) * _cameraDragSpeed * Time.deltaTime,
                0.5f);
            _lastMousePosition = Mouse.current.position.ReadValue();
        }
    }

    private void EdgePan()
    {
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
}

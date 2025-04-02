using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private float _cameraMovementSpeed = 3;
    [SerializeField] private float _cameraDragSpeed = 1;
    [SerializeField] private float _cameraZoomSpeed = 7;

    private InputSystem_Actions _inputActions;

    //Camera
    private Vector2 _cameraMovement;
    private float _cameraZoom;
    //Drag
    private bool _isMouseDragging;
    private Vector2 _lastMousePosition;

    private void OnEnable() => _inputActions.Player.Enable();
    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.CameraMovement.performed += ctx => _cameraMovement = ctx.ReadValue<Vector2>().normalized;
        _inputActions.Player.CameraMovement.canceled += ctx => _cameraMovement = Vector2.zero;
        _inputActions.Player.CameraZoom.performed += ctx => _cameraZoom = -ctx.ReadValue<Vector2>().y;
        _inputActions.Player.CameraZoom.canceled += ctx => _cameraZoom = 0.0f;

        _inputActions.Player.RightClick.started += ctx => StartDragging();
        _inputActions.Player.RightClick.canceled += ctx => _isMouseDragging = false;
        _inputActions.Player.MouseMovement.performed += ctx => DragCamera();
    }

    private void Update()
    {
        if (!_isMouseDragging)
        {
            KeyMovement();
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
}

using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private float _cameraMovementSpeed = 6;
    [SerializeField] private float _cameraZoomSpeed = 1;
    
    private InputSystem_Actions _inputActions;
    private Vector2 _cameraMovement;
    private float _cameraZoom;

    private void OnEnable()
    {
        _inputActions.Player.Enable();
    }

    private void OnDisable() => _inputActions.Player.Disable();

    protected override void OnAwake()
    {
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.CameraMovement.performed += ctx => _cameraMovement = ctx.ReadValue<Vector2>().normalized;
        _inputActions.Player.CameraMovement.canceled += ctx => _cameraMovement = Vector2.zero;
        _inputActions.Player.CameraZoom.performed += ctx => _cameraZoom = -ctx.ReadValue<Vector2>().y;
        _inputActions.Player.CameraZoom.canceled += ctx => _cameraZoom = 0.0f;

    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position +
            new Vector3(_cameraMovement.x, 0, _cameraMovement.y), _cameraMovementSpeed * Time.deltaTime);

        transform.position = new Vector3(
            transform.position.x,
            Mathf.Clamp(Mathf.Lerp(transform.position.y, transform.position.y + _cameraZoom, _cameraZoomSpeed * Time.deltaTime), 2.0f, 20.0f),
            transform.position.z
            );
    }
}

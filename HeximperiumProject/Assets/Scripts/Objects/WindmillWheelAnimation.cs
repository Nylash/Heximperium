using UnityEngine;

public class WindmillWheelAnimation : MonoBehaviour
{
    [SerializeField] private GlobalWindValue _globalWind;
    [SerializeField] private Vector2 _rotationSpeedMultiplierRange;

    private float _rotationSpeedMultiplier;
    private float _currentAngle;

    private void Start()
    {
        _rotationSpeedMultiplier = Random.Range(_rotationSpeedMultiplierRange.x, _rotationSpeedMultiplierRange.y);
    }

    private void Update()
    {
        _currentAngle += _globalWind.windValue * _rotationSpeedMultiplier * Time.deltaTime;
        _currentAngle %= 360f;
        transform.localRotation = Quaternion.Euler(0, 0, _currentAngle);
    }
}

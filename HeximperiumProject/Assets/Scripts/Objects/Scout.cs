using UnityEngine;

public class Scout : MonoBehaviour
{
    private ScoutData _data;
    private Direction _direction;
    private Animator _animator;

    public ScoutData Data { get => _data; set => _data = value; }
    public Direction Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            UpdateCursor();
            if (!ExplorationManager.Instance.ChoosingScoutDirection)
                _animator.SetTrigger("DirectionConfirmed");
        }
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void UpdateCursor()
    {
        switch (_direction)
        {
            case Direction.TopRight:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case Direction.Right:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -60);
                break;
            case Direction.BottomRight:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -120);
                break;
            case Direction.BottomLeft:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -180);
                break;
            case Direction.Left:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -240);
                break;
            case Direction.TopLeft:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -300);
                break;
        }
    }
}

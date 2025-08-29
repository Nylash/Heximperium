using UnityEngine;
using UnityEngine.UI;

public class UIPhase_SpriteRotator : MonoBehaviour
{
    [SerializeField] private Image _north; // 1
    [SerializeField] private Image _west;  // 2
    [SerializeField] private Image _south; // 3
    [SerializeField] private Image _east;  // 4
    [SerializeField] private Sprite _scoreImg;

    private Image[] _positions;
    private Animator _animator;
    private bool _firstRotation = true;
    void Start()
    {
        _animator = GetComponent<Animator>();

        _positions = new Image[4] { _north, _west, _south, _east };

        GameManager.Instance.OnExplorationPhaseEnded += () => StartAnimation();
        GameManager.Instance.OnExpansionPhaseEnded += () => StartAnimation();
        GameManager.Instance.OnExploitationPhaseEnded += () => StartAnimation();
        GameManager.Instance.OnLastTurnStarted += () => OnLastTurn();
    }

    private void StartAnimation()
    {
        UIManager.Instance.UiPhaseInAnimation = true;
        _animator.SetTrigger("Rotate");
    }

    private void OnLastTurn()
    {
        GameManager.Instance.OnExpansionPhaseEnded += () => SetScoreImg();
        GameManager.Instance.OnExploitationPhaseEnded += () => HideLastSprite();
    }

    private void SetScoreImg()
    {
        _positions[2].sprite = _scoreImg;
        _positions[2].color = UIManager.Instance.ColorEntertain;
    }

    private void HideLastSprite()
    {
        _positions[2].enabled = false;
        _positions[1].GetComponent<UIPhase_GetAssociadtedArrow>().AssociatedArrow.enabled = false;
    }

    public void OnRotationEnd()
    {
        UIManager.Instance.UiPhaseInAnimation = false;

        if (_firstRotation)
        {
            _firstRotation = false;
            _east.GetComponent<UIPhase_GetAssociadtedArrow>().AssociatedArrow.enabled = true;
            _east.enabled = true;
        }

        // After a clockwise rotation, the positions shift:
        RotateArrayClockwise();

        if (GameManager.Instance.LastTurn)
            return;
        // Replace south sprite with east sprite
        _positions[2].sprite = _positions[3].sprite;
        _positions[2].color = _positions[3].color;
    }

    // 90° CW in world space => [N,W,S,E] becomes [W,S,E,N]
    private void RotateArrayClockwise()
    {
        var first = _positions[0];          // old North
        for (int i = 0; i < _positions.Length - 1; i++)
            _positions[i] = _positions[i + 1];

        _positions[_positions.Length - 1] = first; // old North moves to East
    }
}

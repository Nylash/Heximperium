using UnityEngine;
using UnityEngine.UI;

public class UIPhase_SpriteRotator : MonoBehaviour
{
    [SerializeField] private Image _north;
    [SerializeField] private Image _west;
    [SerializeField] private Image _south;
    [SerializeField] private Image _east;

    [SerializeField] private Sprite _scoreImg;
    [SerializeField] private Sprite _exploreImg;
    [SerializeField] private Sprite _expandImg;
    [SerializeField] private Sprite _exploitImg;

    private Image[] _slots;

    private int _southIndex = 2;

    private Animator _animator;
    private bool _firstRotation = true;
    private bool _inTurnWithEntertainPhase = false;
    private bool _waitingNextExplorePhaseEnd = false;
    private bool _isLastTurn = false;

    private Image SlotAt(int logicalOffset)
    {
        // logicalOffset: 0 = North, 1 = West, 2 = South, 3 = East
        // On wrappe pour retrouver le bon slot physique
        return _slots[(_southIndex - 2 + logicalOffset + _slots.Length * 4) % _slots.Length];
    }

    private Image CurrentSouth => _slots[_southIndex % _slots.Length];

    void Start()
    {
        _animator = GetComponent<Animator>();
        _slots = new Image[4] { _north, _west, _south, _east };

        ExplorationManager.Instance.OnPhaseFinalized += () => StartAnimation();
        ExpansionManager.Instance.OnPhaseFinalized += () => StartAnimation();
        ExploitationManager.Instance.OnPhaseFinalized += () => StartAnimation();
        EntertainmentManager.Instance.OnPhaseFinalized += () => StartAnimation();

        GameManager.Instance.OnTurnWithEntertainPhase += () => _inTurnWithEntertainPhase = true;
        GameManager.Instance.OnLastTurnStarted += () => _isLastTurn = true;
    }

    private void StartAnimation()
    {
        if (_isLastTurn && GameManager.Instance.CurrentPhase == Phase.Entertain)
            return;

        UIManager.Instance.UiPhaseInAnimation = true;
        _animator.SetTrigger("Rotate");
    }

    public void OnRotationEnd()
    {
        UIManager.Instance.UiPhaseInAnimation = false;

        // Avancer le pointeur Sud (rotation CW : N→W→S→E→N)
        _southIndex = (_southIndex + 1) % _slots.Length;

        if (_firstRotation)
        {
            _firstRotation = false;
            CurrentSouth.GetComponent<UIPhase_GetAssociadtedArrow>().AssociatedArrow.enabled = true;
            CurrentSouth.enabled = true;
        }

        if (_inTurnWithEntertainPhase && GameManager.Instance.CurrentPhase == Phase.Explore)
        {
            SetSouthSprite(_scoreImg, UIManager.Instance.ColorEntertain);
            _inTurnWithEntertainPhase = false;
            _waitingNextExplorePhaseEnd = true;
            return;
        }

        if (_isLastTurn && GameManager.Instance.CurrentPhase == Phase.Expand)
            HideSpritesForLastTurn();

        if (_waitingNextExplorePhaseEnd && GameManager.Instance.CurrentPhase != Phase.Explore)
            return;

        Image currentEast = SlotAt(3);
        SetSouthSprite(currentEast.sprite, currentEast.color);
        _waitingNextExplorePhaseEnd = false;
    }

    private void SetSouthSprite(Sprite sprite, Color color)
    {
        CurrentSouth.sprite = sprite;
        CurrentSouth.color = color;
    }

    public void HideSpritesForLastTurn()
    {
        CurrentSouth.enabled = false;
        Image currentWest = SlotAt(1);
        currentWest.GetComponent<UIPhase_GetAssociadtedArrow>().AssociatedArrow.enabled = false;
    }
}

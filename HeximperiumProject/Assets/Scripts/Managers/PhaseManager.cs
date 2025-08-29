using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class PhaseManager<T> : Singleton<T> where T : MonoBehaviour
{
    protected List<Vector3> _interactionPositions = new List<Vector3>();
    protected List<GameObject> _buttons = new List<GameObject>();

    //Variables for animated tiles
    protected HashSet<Tile> _animatedTiles = new HashSet<Tile>();
    protected Coroutine _interactableTilesCoroutine;
    protected bool _animWasPlaying = false;
    protected AnimatorStateInfo _stateInfo = default;
    protected float _progress = 0f;
    protected bool _syncAnimationFromPreviousPhase = true;

    public bool AnimWasPlaying { get => _animWasPlaying; }
    public AnimatorStateInfo StateInfo { get => _stateInfo; }
    public float Progress { get => _progress; }

    public event Action OnPhaseFinalized;

    protected abstract void StartPhase();
    protected abstract void ConfirmPhase();

    protected IEnumerator PhaseFinalized()
    {
        // Wait for one frame
        yield return null;

        OnPhaseFinalized?.Invoke();
    }

    protected abstract void NewTileSelected(Tile tile);

    protected void TileUnselected()
    {
        if (PopUpManager.Instance.ClonedButton)
        {
            PopUpManager.Instance.ClonedButton.DestroyHighlightedClone();
            PopUpManager.Instance.ClonedButton = null;
        }
        foreach (GameObject button in _buttons)
        {
            button.GetComponent<InteractionButton>().DestroyInteractionButton();
        }
        _buttons.Clear();
    }

    public void ButtonsFade(bool fade)
    {
        foreach (GameObject item in _buttons)
        {
            item.GetComponent<InteractionButton>().FadeAnimation(fade);
        }
    }

    public abstract void AnimateInteractableTiles();

    public void EndPhaseStopAnimation()
    {
        SyncAnimationInteractableTiles();
        StopAnimationInteractableTiles();
        _syncAnimationFromPreviousPhase = true;
    }

    protected void StopAnimationInteractableTiles()
    {
        if (_interactableTilesCoroutine != null)
        {
            StopCoroutine(_interactableTilesCoroutine);
            _interactableTilesCoroutine = null;
        }

        foreach (Tile tile in _animatedTiles)
        {
            tile.Animator.SetBool("Interactable", false);
        }
        _animatedTiles.Clear();
    }

    protected IEnumerator PlayInteractableAnimation(bool animWasPlaying, float progress, AnimatorStateInfo stateInfo)
    {
        yield return new WaitForEndOfFrame();
        foreach (Tile tile in _animatedTiles)
        {
            tile.Animator.SetBool("Interactable", true);
            if (animWasPlaying)
                tile.Animator.Play(stateInfo.shortNameHash, 0, progress);
        }
        _interactableTilesCoroutine = null;
    }

    protected void SyncAnimationInteractableTiles()
    {
        _animWasPlaying = false;
        _stateInfo = default;
        _progress = 0f;
        if (_animatedTiles.Count > 0)
        {
            _stateInfo = _animatedTiles.FirstOrDefault().Animator.GetCurrentAnimatorStateInfo(0);
            _progress = _stateInfo.normalizedTime % 1f;
            _animWasPlaying = true;
        }
    }
}

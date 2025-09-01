using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PhaseManager<T> : Singleton<T> where T : MonoBehaviour
{
    protected List<Vector3> _interactionPositions = new List<Vector3>();
    protected List<GameObject> _buttons = new List<GameObject>();

    //Variables for animated tiles
    protected HashSet<Tile> _animatedTiles = new HashSet<Tile>();
    protected Dictionary<Tile, Vector3> _initialPositions = new Dictionary<Tile, Vector3>();
    protected Coroutine _interactableTilesCoroutine;
    protected bool _animWasPlaying = false;
    protected float _progress = 0f;
    protected bool _syncAnimationFromPreviousPhase = true;
    protected float _bounceStartTime = 0f;
    protected const float _bouncePeriod = 1f;
    protected const float _bounceHeight = 0.1f;

    public bool AnimWasPlaying { get => _animWasPlaying; }
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
            if (_initialPositions.TryGetValue(tile, out Vector3 pos))
            {
                tile.Visual.localPosition = pos;
            }
        }
        _animatedTiles.Clear();
        _initialPositions.Clear();
    }

    protected IEnumerator PlayInteractableAnimation(bool animWasPlaying, float progress)
    {
        yield return null;
        _bounceStartTime = Time.time - progress * _bouncePeriod;
        foreach (Tile tile in _animatedTiles)
        {
            if (!_initialPositions.ContainsKey(tile))
                _initialPositions[tile] = tile.Visual.localPosition;
        }

        while (_animatedTiles.Count > 0)
        {
            float phase = (Time.time - _bounceStartTime) / _bouncePeriod;
            float offset = Mathf.Sin(phase * Mathf.PI * 2f) * _bounceHeight;
            foreach (Tile tile in _animatedTiles)
            {
                if (_initialPositions.TryGetValue(tile, out Vector3 basePos))
                {
                    tile.Visual.localPosition = new Vector3(basePos.x, basePos.y + offset, basePos.z);
                }
            }
            yield return null;
        }
        _interactableTilesCoroutine = null;
    }

    protected void SyncAnimationInteractableTiles()
    {
        _animWasPlaying = _animatedTiles.Count > 0;
        if (_animWasPlaying)
        {
            _progress = ((Time.time - _bounceStartTime) / _bouncePeriod) % 1f;
        }
        else
        {
            _progress = 0f;
        }
    }
}

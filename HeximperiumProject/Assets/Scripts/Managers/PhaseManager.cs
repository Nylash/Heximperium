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
    private Dictionary<Tile, Coroutine> _returnCoroutines = new Dictionary<Tile, Coroutine>();
    protected bool _animWasPlaying = false;
    protected float _progress = 0f;
    protected bool _syncAnimationFromPreviousPhase = true;
    protected float _bounceStartTime = 0f;
    protected const float _bouncePeriod = 1f;
    protected const float _bounceHeight = 0.1f;
    protected const float _returnDuration = 0.15f;

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
        StopAnimationInteractableTiles(true);
        _syncAnimationFromPreviousPhase = true;
    }

    protected void StopAnimationInteractableTiles(bool smoothReturn = false)
    {
        if (_interactableTilesCoroutine != null)
        {
            StopCoroutine(_interactableTilesCoroutine);
            _interactableTilesCoroutine = null;
        }
        if (!smoothReturn)
        {
            foreach (var kvp in _returnCoroutines)
            {
                StopCoroutine(kvp.Value);
            }
            _returnCoroutines.Clear();
        }
        if (smoothReturn && _animatedTiles.Count > 0)
        {
            _interactableTilesCoroutine = StartCoroutine(ReturnTilesToInitialPositions());
        }
        else
        {
            foreach (var kvp in _initialPositions)
            {
                kvp.Key.Visual.localPosition = kvp.Value;
            }
            _animatedTiles.Clear();
            _initialPositions.Clear();
        }
    }

    protected void ApplyAnimatedTiles(HashSet<Tile> newTiles)
    {
        var toRemove = new HashSet<Tile>(_animatedTiles);
        toRemove.ExceptWith(newTiles);
        foreach (Tile tile in toRemove)
        {
            _animatedTiles.Remove(tile);
            if (_returnCoroutines.TryGetValue(tile, out Coroutine existing))
            {
                StopCoroutine(existing);
            }
            if (_initialPositions.ContainsKey(tile))
            {
                _returnCoroutines[tile] = StartCoroutine(ReturnTileToInitialPosition(tile));
            }
        }

        foreach (Tile tile in newTiles)
        {
            if (_animatedTiles.Contains(tile))
                continue;
            if (_returnCoroutines.TryGetValue(tile, out Coroutine returning))
            {
                StopCoroutine(returning);
                _returnCoroutines.Remove(tile);
            }
            if (!_initialPositions.ContainsKey(tile))
                _initialPositions[tile] = tile.Visual.localPosition;
            _animatedTiles.Add(tile);
        }

        if (_animatedTiles.Count > 0)
        {
            if (_interactableTilesCoroutine == null)
                _interactableTilesCoroutine = StartCoroutine(PlayInteractableAnimation(_animWasPlaying, _progress));
        }
        else if (_interactableTilesCoroutine != null)
        {
            StopCoroutine(_interactableTilesCoroutine);
            _interactableTilesCoroutine = null;
        }
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

    private IEnumerator ReturnTilesToInitialPositions()
    {
        Dictionary<Tile, Vector3> startPositions = new Dictionary<Tile, Vector3>();
        foreach (Tile tile in _animatedTiles)
        {
            startPositions[tile] = tile.Visual.localPosition;
        }

        float elapsed = 0f;
        while (elapsed < _returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _returnDuration);
            foreach (Tile tile in _animatedTiles)
            {
                if (startPositions.TryGetValue(tile, out Vector3 start) &&
                    _initialPositions.TryGetValue(tile, out Vector3 target))
                {
                    tile.Visual.localPosition = Vector3.Lerp(start, target, t);
                }
            }
            yield return null;
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
        _interactableTilesCoroutine = null;
    }

    private IEnumerator ReturnTileToInitialPosition(Tile tile)
    {
        Vector3 start = tile.Visual.localPosition;
        if (!_initialPositions.TryGetValue(tile, out Vector3 target))
            yield break;
        float elapsed = 0f;
        while (elapsed < _returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _returnDuration);
            tile.Visual.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }
        tile.Visual.localPosition = target;
        _initialPositions.Remove(tile);
        _returnCoroutines.Remove(tile);
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

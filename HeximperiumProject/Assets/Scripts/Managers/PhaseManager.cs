using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PhaseManager<T> : Singleton<T> where T : MonoBehaviour
{
    protected List<Vector3> _interactionPositions = new List<Vector3>();
    protected List<GameObject> _buttons = new List<GameObject>();

    //Variables for animated tiles
    private HashSet<Tile> _animatedTiles = new HashSet<Tile>();
    private HashSet<Tile> _stoppingAnimationTiles = new HashSet<Tile>();
    private bool _animWasPlaying = false;
    private float _progress = 0f;
    private Dictionary<Tile, float> _bounceStartTimes = new Dictionary<Tile, float>();
    private const float _bouncePeriod = 1f;
    private const float _bounceHeight = 0.05f;
    private const float _returnDuration = 0.15f;// Make sure this is lower than UIPhase animation rotation duration to avoid needing sync between phases

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

    #region TILE INTERACTION ANIMATION
    public abstract void AnimateInteractableTiles();

    protected void StopAllAnimations(bool endOfPhase = false)
    {
        while (_animatedTiles.Count > 0)
        {
            Tile tile = _animatedTiles.First();
            _stoppingAnimationTiles.Add(tile);
            tile.InteractionAnimationState = TileInteractionAnimationState.Stopping;
            _animatedTiles.Remove(tile);
            _bounceStartTimes.Remove(tile);
        }

        if (endOfPhase)
        {
            foreach (Tile tile in _stoppingAnimationTiles)
            {
                tile.InteractionCoroutine = StartCoroutine(StopAnimationInteraction(tile));
            }
        }
    }

    protected void LaunchAnimation(HashSet<Tile> tiles)
    {
        if (_stoppingAnimationTiles.Count == 0)
        {
            foreach (Tile tile in tiles)
            {
                _animatedTiles.Add(tile);
                tile.InteractionAnimationState = TileInteractionAnimationState.Animating;
                if (!_bounceStartTimes.ContainsKey(tile))
                {
                    _bounceStartTimes[tile] = Time.time - _progress * _bouncePeriod;
                }
                tile.InteractionCoroutine = StartCoroutine(AnimationInteraction(tile));
            }
        }
        else
        {
            foreach (Tile tile in tiles)
            {
                if (_stoppingAnimationTiles.Contains(tile))
                {
                    _stoppingAnimationTiles.Remove(tile);
                }
                _animatedTiles.Add(tile);
                tile.InteractionAnimationState = TileInteractionAnimationState.Animating;
                if (!_bounceStartTimes.ContainsKey(tile))
                {
                    _bounceStartTimes[tile] = Time.time - _progress * _bouncePeriod;
                }
                tile.InteractionCoroutine = StartCoroutine(AnimationInteraction(tile));
            }
            foreach (Tile tile in _stoppingAnimationTiles)
            {
                tile.InteractionCoroutine = StartCoroutine(StopAnimationInteraction(tile));
            }
        }
    }

    private IEnumerator StopAnimationInteraction(Tile tile)
    {
        float elapsed = 0f;
        Vector3 start = tile.Visual.localPosition;
        while (elapsed < _returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _returnDuration);
            tile.Visual.localPosition = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }

        tile.Visual.localPosition = Vector3.zero;// Correct any floating point errors
        tile.InteractionCoroutine = null;
        tile.InteractionAnimationState = TileInteractionAnimationState.None;
        _stoppingAnimationTiles.Remove(tile);
    }

    private IEnumerator AnimationInteraction(Tile tile)
    {
        Vector3 basePos = tile.Visual.localPosition;

        yield return null;

        float startTime = _bounceStartTimes[tile];

        while (tile.InteractionAnimationState == TileInteractionAnimationState.Animating)
        {
            float phase = (Time.time - startTime) / _bouncePeriod;
            float offset = Mathf.Sin(phase * Mathf.PI * 2f) * _bounceHeight;
            tile.Visual.localPosition = basePos + new Vector3(0f, offset, 0f);
            yield return null;
        }
    }

    protected void SyncAnimationInteractableTiles()
    {
        _animWasPlaying = _animatedTiles.Count > 0;
        if (_animWasPlaying)
        {
            Tile tile = _animatedTiles.First();
            if (_bounceStartTimes.TryGetValue(tile, out float startTime))
            {
                _progress = ((Time.time - startTime) / _bouncePeriod) % 1f;
            }
            else
            {
                _progress = 0f;
            }
        }
        else
        {
            _progress = 0f;
        }
    }
    #endregion
}

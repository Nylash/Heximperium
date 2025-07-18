using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PhaseManager<T> : Singleton<T> where T : MonoBehaviour
{
    protected List<Vector3> _interactionPositions = new List<Vector3>();
    protected List<GameObject> _buttons = new List<GameObject>();

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
}

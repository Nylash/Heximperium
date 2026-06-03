using System.Collections;
using TMPro;
using UnityEngine;

public class UI_ScoreCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private string _prefix = "";
    [SerializeField] private string _spriteSuffix = "<sprite name=\"Point_Emoji\">";

    private Coroutine _currentCoroutine;

    public void CountTo(int targetValue, float duration)
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        if (targetValue == 0 || duration <= 0f)
        {
            SetText(0);
            return;
        }

        _currentCoroutine = StartCoroutine(CountRoutine(targetValue, duration));
    }

    private IEnumerator CountRoutine(int target, float duration)
    {
        float elapsed = 0f;
        SetText(0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetText(Mathf.RoundToInt(t * target));
            yield return null;
        }

        SetText(target);
        _currentCoroutine = null;
    }

    private void SetText(int value)
    {
        _label.text = $"{_prefix}{value}{_spriteSuffix}";
    }
}

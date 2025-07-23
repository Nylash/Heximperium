using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.UI;

public class LoadingManager : Singleton<LoadingManager>
{
    [SerializeField] private float _minLoadingTime = 2f;
    [SerializeField] private CanvasGroup _loadingUI;
    [SerializeField] private Image _progress;

    public event Action OnLoadingDone;

    private string _targetSceneName;
    private bool _isLoaded;

    protected override void OnAwake()
    {
        Utilities.OnGameInitialized += () => _isLoaded = true;
    } 

    public void StartLoading(string targetScene)
    {
        _targetSceneName = targetScene;
        _loadingUI.alpha = 1;
        _progress.fillAmount = 0f;
        _loadingUI.gameObject.SetActive(true);
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        float elapsed = 0f;

        while (elapsed < 1f) //Wait for the previous scene to be fully unloaded
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        var op = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = true;

        elapsed = 0f;

        while (!op.isDone || elapsed < _minLoadingTime)
        {
            _progress.fillAmount = elapsed / _minLoadingTime;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Scene loadedScene = SceneManager.GetSceneByName(_targetSceneName);
        SceneManager.SetActiveScene(loadedScene);

        // if the game init event hasn’t fired yet, wait here
        if (!_isLoaded)
            yield return new WaitUntil(() => _isLoaded);

        FinishLoading();
    }

    private void FinishLoading()
    {
        StartCoroutine(DoCleanup());
        Utilities.OnGameInitialized -= FinishLoading;

        OnLoadingDone?.Invoke();
    }

    private IEnumerator DoCleanup()
    {
        // Fade‑out
        float t = 0f, d = 0.3f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            _loadingUI.alpha = 1 - (t / d);
            yield return null;
        }

        Destroy(_loadingUI.gameObject);
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}

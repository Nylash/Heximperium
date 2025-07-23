using System;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    [Header("_________________________________________________________")]
    [Header("Intro")]
    [SerializeField] private GameObject _introduction;
    [Header("_________________________________________________________")]
    [Header("Exploration Turn 1")]
    [SerializeField] private GameObject _explo1;
    [SerializeField] private GameObject _explo1_1;//Select town
    [SerializeField] private GameObject _explo1_2;//Scout spawned
    [SerializeField] private GameObject _explo1_3;//Scout directed
    [SerializeField] private GameObject _explo1_4;//Phase ended

    public event Action OnTutorialStarted;

    protected override void OnAwake()
    {
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.OnLoadingDone += () => _introduction.SetActive(true);
        else
            _introduction.SetActive(true);
    }

    public void StartTutorial()
    {
        _introduction.GetComponent<Animator>().SetTrigger("Shrink");
        OnTutorialStarted?.Invoke();
        InitializeExplo1();
    }

    #region EXPLORATION 1
    private void InitializeExplo1()
    {
        _explo1.SetActive(true);
        GameManager.Instance.GamePaused = true;
    }

    public void StartExplo1()
    {
        _explo1.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _explo1_1.SetActive(true);
        ExplorationManager.Instance.OnTownSelected += Explo1_1Done;

        GameManager.Instance.OnTileUnselected += RollBackToExplo1_1;
    }

    private void RollBackToExplo1_1()
    {
        _explo1_2.SetActive(false);
        _explo1_1.SetActive(true);
    }

    private void Explo1_1Done()
    {
        _explo1_1.SetActive(false);
        _explo1_2.SetActive(true);
        ExplorationManager.Instance.OnScoutSpawned += scout => Explo1_2Done();
    }

    private void Explo1_2Done()
    {
        _explo1_1.SetActive(false);
        _explo1_2.SetActive(false);
        _explo1_3.SetActive(true);
        ExplorationManager.Instance.OnScoutSpawned -= scout => Explo1_2Done();
        ExplorationManager.Instance.OnTownSelected -= Explo1_1Done;
        GameManager.Instance.OnTileUnselected -= RollBackToExplo1_1;
        ExplorationManager.Instance.OnScoutDirected += Explo1_3Done;
    }

    private void Explo1_3Done()
    {
        _explo1_3.SetActive(false);
        _explo1_4.SetActive(true);
        ExplorationManager.Instance.OnScoutDirected -= Explo1_3Done;
        GameManager.Instance.OnExplorationPhaseEnded += Explo1_4Done;
    }

    private void Explo1_4Done()
    {
        _explo1_4.SetActive(false);
        GameManager.Instance.OnExplorationPhaseEnded -= Explo1_4Done;
    }
    #endregion
}

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
    [SerializeField] private Animator _explo1_1;//Select town
    [SerializeField] private Animator _explo1_2;//Scout spawned
    [SerializeField] private Animator _explo1_3;//Scout directed
    [SerializeField] private Animator _explo1_4;//Phase ended
    [Header("_________________________________________________________")]
    [Header("Expansion Turn 1")]
    [SerializeField] private GameObject _expand1;
    [SerializeField] private Animator _expand1_1;//Select tile
    [SerializeField] private Animator _expand1_2;//Claim tile
    [SerializeField] private Animator _expand1_3;//Phase ended

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
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;

        _explo1.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _explo1_1.SetTrigger("Unfold");
        ExplorationManager.Instance.OnTownSelected += Explo1_1Done;
    }

    private void Explo1_1Done()
    {
        GameManager.Instance.OnTileUnselected += RollBackToExplo1_1;
        _explo1_1.SetTrigger("Fold");
        _explo1_2.SetTrigger("Unfold");
        ExplorationManager.Instance.OnScoutSpawned += scout => Explo1_2Done();
    }

    private void RollBackToExplo1_1()
    {
        GameManager.Instance.OnTileUnselected -= RollBackToExplo1_1;
        _explo1_2.SetTrigger("Fold");
        _explo1_1.SetTrigger("Unfold");
    }

    private void Explo1_2Done()
    {
        ExplorationManager.Instance.OnScoutSpawned -= scout => Explo1_2Done();
        ExplorationManager.Instance.OnTownSelected -= Explo1_1Done;
        GameManager.Instance.OnTileUnselected -= RollBackToExplo1_1;
        _explo1_1.SetTrigger("Fold");
        _explo1_2.SetTrigger("Fold");
        _explo1_3.SetTrigger("Unfold");
        ExplorationManager.Instance.OnScoutDirected += Explo1_3Done;
    }

    private void Explo1_3Done()
    {
        _explo1_3.SetTrigger("Fold");
        _explo1_4.SetTrigger("Unfold");
        ExplorationManager.Instance.OnScoutDirected -= Explo1_3Done;
        GameManager.Instance.OnExplorationPhaseEnded += Explo1_4Done;

        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
    }

    private void Explo1_4Done()
    {
        _explo1_4.SetTrigger("Fold");
        GameManager.Instance.OnExplorationPhaseEnded -= Explo1_4Done;

        GameManager.Instance.OnExpansionPhaseStarted += InitializeExpand1;
    }
    #endregion

    #region EXPANSION 1
    private void InitializeExpand1()
    {
        _expand1.SetActive(true);
        GameManager.Instance.GamePaused = true;
    }

    public void StartExpand1()
    {
        UIManager.Instance.ButtonEndPhase.interactable = false;
        GameManager.Instance.TutorialLockingPhase = true;

        _expand1.GetComponent<Animator>().SetTrigger("Shrink");
        GameManager.Instance.GamePaused = false;

        _expand1_1.SetTrigger("Unfold");
        ExpansionManager.Instance.OnClaimableTileSelected += Expand1_1Done;
    }

    private void Expand1_1Done()
    {
        GameManager.Instance.OnTileUnselected += RollBackToExpand1_1;
        _expand1_1.SetTrigger("Fold");
        _expand1_2.SetTrigger("Unfold");
        ExpansionManager.Instance.OnTileClaimed+= tile => Expand1_2Done();
    }

    private void RollBackToExpand1_1()
    {
        GameManager.Instance.OnTileUnselected -= RollBackToExpand1_1;
        _expand1_2.SetTrigger("Fold");
        _expand1_1.SetTrigger("Unfold");
    }

    private void Expand1_2Done()
    {
        _expand1_2.SetTrigger("Fold");
        _expand1_3.SetTrigger("Unfold");
        GameManager.Instance.OnExpansionPhaseEnded += Expand1_3Done;
        UIManager.Instance.ButtonEndPhase.interactable = true;
        GameManager.Instance.TutorialLockingPhase = false;
    }

    private void Expand1_3Done()
    {
        _expand1_3.SetTrigger("Fold");
        GameManager.Instance.OnExpansionPhaseEnded -= Expand1_3Done;

        //GameManager.Instance.OnExploitationPhaseStarted += InitializeExploit1;
    }
    #endregion
}

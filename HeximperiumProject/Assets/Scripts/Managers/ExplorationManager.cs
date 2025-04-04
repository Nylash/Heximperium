public class ExplorationManager : Singleton<ExplorationManager>
{
    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
    }

    private void StartPhase(Phase phase)
    {
        if (phase != Phase.Explore)
            return;
        print("Start Exploration");
    }
}

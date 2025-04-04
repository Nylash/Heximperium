public class ExpansionManager : Singleton<ExpansionManager>
{
    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
    }

    public void StartPhase(Phase phase)
    {
        if (phase != Phase.Expand)
            return;
        print("Start Expansion");
        ResourcesManager.Instance.Claim += 5;
    }
}

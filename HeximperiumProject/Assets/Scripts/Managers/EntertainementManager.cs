public class EntertainementManager : Singleton<EntertainementManager>
{
    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
    }

    public void StartPhase(Phase phase)
    {
        if (phase != Phase.Entertain)
            return;
        print("Start Entertainement");
    }
}

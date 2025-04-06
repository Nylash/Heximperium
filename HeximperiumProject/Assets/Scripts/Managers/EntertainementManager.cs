public class EntertainementManager : Singleton<EntertainementManager>
{
    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
        GameManager.Instance.event_newTileSelected.AddListener(NewTileSelected);
    }

    private void StartPhase(Phase phase)
    {
        if (phase != Phase.Entertain)
            return;
        print("Start Entertainement");
    }

    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Entertain)
            return;
    }
}

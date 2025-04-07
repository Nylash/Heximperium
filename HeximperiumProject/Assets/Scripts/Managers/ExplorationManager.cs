public class ExplorationManager : Singleton<ExplorationManager>
{
    protected override void OnAwake()
    {
        GameManager.Instance.event_newPhase.AddListener(StartPhase);
        GameManager.Instance.event_newTileSelected.AddListener(NewTileSelected);
    }

    private void StartPhase(Phase phase)
    {
        if (phase != Phase.Explore)
            return;
    }

    private void NewTileSelected(Tile tile)
    {
        if (GameManager.Instance.CurrentPhase != Phase.Explore)
            return;
    }
}

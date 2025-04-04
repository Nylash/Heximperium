public class ExpansionManager : Singleton<ExpansionManager>
{
    public void StartPhase()
    {
        print("Start Expansion");
        ResourcesManager.Instance.Claim += 5;
    }
}

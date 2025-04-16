[System.Serializable]
public class InfraAvailableCopy
{
    public InfrastructureData infrastructure;
    public int availableCopy;

    public InfraAvailableCopy(InfrastructureData i, int a)
    {
        infrastructure = i;
        availableCopy = a;
    }
}

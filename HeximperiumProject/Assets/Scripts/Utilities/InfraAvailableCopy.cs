[System.Serializable]
//Class used to represent the number of copy of an infrastructure
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

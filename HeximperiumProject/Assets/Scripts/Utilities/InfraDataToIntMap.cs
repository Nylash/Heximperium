[System.Serializable]
//Class used to represent the number of copy of an infrastructure
public class InfraDataToIntMap
{
    public InfrastructureData infrastructure;
    public int availableCopy;

    public InfraDataToIntMap(InfrastructureData i, int a)
    {
        infrastructure = i;
        availableCopy = a;
    }
}

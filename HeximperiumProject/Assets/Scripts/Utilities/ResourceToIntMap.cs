[System.Serializable]
//Class used to represent costs and incomes
public class ResourceToIntMap
{
    public Resource resource;
    public int value;

    public ResourceToIntMap(Resource r, int v)
    {
        resource = r;
        value = v;
    }
}

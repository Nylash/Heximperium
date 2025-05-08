[System.Serializable]
//Class used to represent costs and incomes
public class ResourceValue
{
    public Resource resource;
    public int value;

    public ResourceValue(Resource r, int v)
    {
        resource = r;
        value = v;
    }
}

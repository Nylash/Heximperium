public abstract class UI_DynamicPopUp : UI_PopUp
{
    public abstract void InitializePopUp<T>(T item);

    public override void DestroyPopUp()
    {
        Destroy(gameObject);
    }
}

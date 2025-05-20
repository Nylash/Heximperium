public abstract class UI_ResourcePopUp : UI_PopUp
{
    public override void DestroyPopUp()
    {
        Destroy(gameObject);
    }

    public abstract void InitializePopUp();
}

using UnityEngine;

public abstract class UI_DynamicPopUp : MonoBehaviour
{
    public abstract void InitializePopUp<T>(T item);
}

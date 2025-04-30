using UnityEngine;

public class Entertainer : MonoBehaviour
{
    [SerializeField] private EntertainerData _entertainerData;

    public EntertainerData EntertainerData { get => _entertainerData; set => _entertainerData = value; }
}

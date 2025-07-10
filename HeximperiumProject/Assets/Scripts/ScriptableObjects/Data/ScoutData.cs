using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scout")]
public class ScoutData : ScriptableObject
{
    [SerializeField] private int _speed;
    [SerializeField] private int _lifespan;
    [SerializeField] private int _revealRadius;

    public int Speed { get => _speed;}
    public int Lifespan { get => _lifespan;}
    public int RevealRadius { get => _revealRadius;}
}

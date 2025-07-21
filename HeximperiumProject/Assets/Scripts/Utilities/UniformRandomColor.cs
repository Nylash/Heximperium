using UnityEngine;

public class UniformRandomColor : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    private void Start()
    {
        ParticleSystem.MainModule main = _particleSystem.main;
        ParticleSystem.MinMaxGradient grad = main.startColor;
        Color chosenColor = Color.white;

        switch (grad.mode)
        {
            case ParticleSystemGradientMode.Color:
                print("Not handled yet");
                break;
            case ParticleSystemGradientMode.Gradient:
                print("Not handled yet");
                break;
            case ParticleSystemGradientMode.TwoColors:
                print("Not handled yet");
                break;
            case ParticleSystemGradientMode.TwoGradients:
                print("Not handled yet");
                break;
            case ParticleSystemGradientMode.RandomColor:
                chosenColor = grad.Evaluate(Random.Range(0f, 1f));
                break;
            default:
                break;
        }

        main.startColor = chosenColor;
        _particleSystem.Play();
    }
}

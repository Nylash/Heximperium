using UnityEngine;

public class UIUpgrade_AnimationEvent : MonoBehaviour
{
    [SerializeField] private Phase _phase;

    public void RerollUpdate()
    {
        UpgradesManager.Instance.RerollUpgradesChoice(_phase);
    }
}

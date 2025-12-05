using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private Slider _masterVolume;

    protected override void OnAwake()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            _masterVolume.value = PlayerPrefs.GetFloat("MasterVolume");
        }
    }

    public void MasterVolumeUpdate(Single value)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.MasterVolume = value;
        }
    }
}

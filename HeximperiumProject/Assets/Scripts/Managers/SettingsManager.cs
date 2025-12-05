using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private Slider _masterVolume;

    protected override void OnAwake()
    {
        if (PlayerPrefs.HasKey(SoundsManager.MASTER_VOLUME_KEY))
        {
            _masterVolume.value = PlayerPrefs.GetFloat(SoundsManager.MASTER_VOLUME_KEY);
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

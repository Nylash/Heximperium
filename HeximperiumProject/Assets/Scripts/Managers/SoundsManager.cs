using System;
using UnityEngine;

public class SoundsManager : Singleton<SoundsManager>
{
    public const string MASTER_VOLUME_KEY = "MasterVolume";

    [SerializeField] private AudioSource _musicSource;

    private float _masterVolume = 1.0f;
    private float _baseMusicVolume;

    public event Action OnMasterVolumeChanged;

    public float MasterVolume { 
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            OnMasterVolumeChanged?.Invoke();
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, _masterVolume);
        }
    }

    protected override void OnAwake()
    {
        _baseMusicVolume = _musicSource.volume;

        OnMasterVolumeChanged += () => _musicSource.volume = _baseMusicVolume * _masterVolume;

        if (PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
        {
            MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
        }
    }
}

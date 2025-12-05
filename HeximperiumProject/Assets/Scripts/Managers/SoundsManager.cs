using System;
using UnityEngine;

public class SoundsManager : Singleton<SoundsManager>
{
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
            PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        }
    }

    protected override void OnAwake()
    {
        _baseMusicVolume = _musicSource.volume;

        OnMasterVolumeChanged += () => _musicSource.volume = _baseMusicVolume * _masterVolume;

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
        }
    }
}

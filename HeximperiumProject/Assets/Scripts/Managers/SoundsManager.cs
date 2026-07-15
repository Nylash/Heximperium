using System;
using UnityEngine;

public class SoundsManager : Singleton<SoundsManager>
{
    public const string MASTER_VOLUME_KEY = "MasterVolume";
    public const string EFFECTS_VOLUME_KEY = "EffectsVolume";
    public const string MUSIC_VOLUME_KEY = "MusicVolume";

    [SerializeField] private AudioSource _musicSource;

    private float _masterVolume = 1.0f;
    private float _effectsVolume = 1.0f;
    private float _musicVolume = 1.0f;

    private float _baseMusicVolume;

    public event Action OnMasterVolumeChanged;
    public event Action OnEffectsVolumeChanged;
    public event Action OnMusicVolumeChanged;

    public float MasterVolume { 
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            OnMasterVolumeChanged?.Invoke();
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, _masterVolume);
        }
    }

    public float EffectsVolume
    {
        get => _effectsVolume;
        set
        {
            _effectsVolume = value;
            OnEffectsVolumeChanged?.Invoke();
            PlayerPrefs.SetFloat(EFFECTS_VOLUME_KEY, _effectsVolume);
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = value;
            OnMusicVolumeChanged?.Invoke();
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, _musicVolume);
        }
    }

    protected override void OnAwake()
    {
        _baseMusicVolume = _musicSource.volume;

        OnMasterVolumeChanged += () => _musicSource.volume = _baseMusicVolume * _masterVolume * _musicVolume;
        OnMusicVolumeChanged += () => _musicSource.volume = _baseMusicVolume * _masterVolume * _musicVolume;

        if (PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
        {
            MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
        }
        if (PlayerPrefs.HasKey(EFFECTS_VOLUME_KEY))
        {
            EffectsVolume = PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY);
        }
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
        {
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
        }
    }
}

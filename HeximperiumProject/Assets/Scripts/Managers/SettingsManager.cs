using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    const string RESOLUTION_WIDTH_KEY = "ScreenWidthResolution";
    const string RESOLUTION_HEIGHT_KEY = "ScreenHeightResolution";
    const string FULLSCREEN_KEY = "Fullscreen";

    [Header("_________________________________________________________")]
    [Header("Audio")]
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _effectsVolume;
    [SerializeField] private Slider _musicVolume;
    [Header("_________________________________________________________")]
    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;

    protected override void OnAwake()
    {
        //GRAPHICS
        FilterAvailableResolutions();
        bool isFullscreen = Screen.fullScreen;

        if (PlayerPrefs.HasKey(FULLSCREEN_KEY))
        {
            isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY) == 1;
        }
        Screen.fullScreen = isFullscreen;
        _fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);

        int width, height;
        if (PlayerPrefs.HasKey(RESOLUTION_WIDTH_KEY) && PlayerPrefs.HasKey(RESOLUTION_HEIGHT_KEY))
        {
            width = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY);
            height = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY);
        }
        else
        {
            width = Screen.currentResolution.width;
            height = Screen.currentResolution.height;
        }

        string label = $"{width}x{height}";
        int index = _resolutionDropdown.options.FindIndex(o => o.text == label);
        if (index >= 0)
            _resolutionDropdown.SetValueWithoutNotify(index);

        Screen.SetResolution(width, height, isFullscreen);
        //MUSIC
        if (PlayerPrefs.HasKey(SoundsManager.MASTER_VOLUME_KEY))
        {
            _masterVolume.value = PlayerPrefs.GetFloat(SoundsManager.MASTER_VOLUME_KEY);
        }
        if (PlayerPrefs.HasKey(SoundsManager.EFFECTS_VOLUME_KEY))
        {
            _effectsVolume.value = PlayerPrefs.GetFloat(SoundsManager.EFFECTS_VOLUME_KEY);
        }
        if (PlayerPrefs.HasKey(SoundsManager.MUSIC_VOLUME_KEY))
        {
            _musicVolume.value = PlayerPrefs.GetFloat(SoundsManager.MUSIC_VOLUME_KEY);
        }
    }

    public void ResetTutorials()
    {
        PlayerPrefs.DeleteKey(PopUpManager.INFRA_LVL_TUTO_KEY);
        PlayerPrefs.DeleteKey(PopUpManager.LOCK_POPUP_TUTO_KEY);
        PlayerPrefs.DeleteKey(PopUpManager.REMOVING_INFRA_TUTO_KEY);
        PlayerPrefs.DeleteKey(PopUpManager.UPGRADE_TUTO_KEY);
        PlayerPrefs.DeleteKey(PopUpManager.SAVINGS_TUTO_KEY);
        PlayerPrefs.DeleteKey(PopUpManager.FILTERS_TUTO_KEY);
        PlayerPrefs.DeleteKey(PopUpManager.TRADE_TUTO_KEY);
        PlayerPrefs.Save();
    }

    #region SOUNDS
    public void MasterVolumeUpdate(Single value)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.MasterVolume = value;
        }
    }

    public void EffectsVolumeUpdate(Single value)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.EffectsVolume = value;
        }
    }

    public void MusicVolumeUpdate(Single value)
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.MusicVolume = value;
        }
    }
    #endregion

    #region CAMERA
    public void CameraMovementSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.CameraMovementSpeed = value;
        }
    }

    public void CameraDragSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.CameraDragSpeed = value;
        }
    }

    public void CameraEdgePanSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.EdgePanSpeed = value;
        }
    }

    public void CameraZoomSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.CameraZoomSpeed = value;
        }
    }
    #endregion

    #region POPUPS
    public void PopUpTimeForLockUpdate(Single value)
    {
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.DurationForLockingPopup = value;
        }
    }

    public void PopUpTimeForSpawnUpdate(Single value)
    {
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.DurationHoverForUI = value;
        }
    }

    public void PopUpSurvivingDurationUpdate(Single value)
    {
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.SurvivablePopupDuration = value;
        }
    }
    #endregion

    #region SCREEN
    public void SetResolution(int index)
    {
        string label = _resolutionDropdown.options[index].text;

        var parts = label.Split('x');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
        {
            Debug.LogError($"Invalid resolution label format: '{label}' (expected 'WIDTHxHEIGHT')");
            return;
        }

        Screen.SetResolution(width, height, Screen.fullScreen);

        PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, width);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, height);
        PlayerPrefs.Save();
    }

    public void SetFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void FilterAvailableResolutions()
    {
        var supportedResolutions = new HashSet<(int width, int height)>(
            Screen.resolutions.Select(r => (r.width, r.height))
        );

        var validOptions = new List<TMP_Dropdown.OptionData>();

        foreach (var option in _resolutionDropdown.options)
        {
            var parts = option.text.Split('x');

            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            {
                if (supportedResolutions.Contains((w, h)))
                {
                    validOptions.Add(option);
                }
            }
            else
            {
                Debug.LogWarning($"Skipping malformed resolution option: '{option.text}'");
            }
        }

        if (validOptions.Count == 0)
        {
            Debug.LogWarning("No Inspector resolution matched Screen.resolutions — keeping full unfiltered list.");
            return; // on ne touche pas à _resolutionDropdown.options, la liste Inspector reste telle quelle
        }

        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(validOptions);
    }
    #endregion
}

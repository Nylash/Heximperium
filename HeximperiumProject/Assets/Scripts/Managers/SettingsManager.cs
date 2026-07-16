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
    public const string CAMERA_MOVEMENT_SPEED_KEY = "CameraMovementSpeed";
    public const string CAMERA_DRAG_SPEED_KEY = "CameraDragSpeed";
    public const string CAMERA_EDGE_PAN_SPEED_KEY = "CameraEdgePanSpeed";
    public const string CAMERA_ZOOM_SPEED_KEY = "CameraZoomSpeed";
    public const string POPUPS_TIME_FOR_LOCK_KEY = "PopupsTimeForLock";
    public const string POPUPS_DURATION_HOVER_KEY = "PopupsDurationHover";
    public const string POPUPS_DURATION_SURVIVING_KEY = "PopupsDurationSurviving";

    [Header("_________________________________________________________")]
    [Header("Audio")]
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _effectsVolume;
    [SerializeField] private Slider _musicVolume;
    [Header("_________________________________________________________")]
    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;
    [Header("_________________________________________________________")]
    [Header("Camera")]
    [SerializeField] private Slider _keyboardPanSpeed;
    [SerializeField] private Slider _edgePanSpeed;
    [SerializeField] private Slider _dragSpeed;
    [SerializeField] private Slider _zoomSpeed;
    [Header("_________________________________________________________")]
    [Header("Popups")]
    [SerializeField] private Slider _popupsTimeForLock;
    [SerializeField] private Slider _popupsDurationHover;
    [SerializeField] private Slider _popupsDurationSurviving;

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
        //CAMERA
        if (PlayerPrefs.HasKey(CAMERA_MOVEMENT_SPEED_KEY))
        {
            _keyboardPanSpeed.value = PlayerPrefs.GetFloat(CAMERA_MOVEMENT_SPEED_KEY);
        }
        if (PlayerPrefs.HasKey(CAMERA_EDGE_PAN_SPEED_KEY))
        {
            _edgePanSpeed.value = PlayerPrefs.GetFloat(CAMERA_EDGE_PAN_SPEED_KEY);
        }
        if (PlayerPrefs.HasKey(CAMERA_DRAG_SPEED_KEY))
        {
            _dragSpeed.value = PlayerPrefs.GetFloat(CAMERA_DRAG_SPEED_KEY);
        }
        if (PlayerPrefs.HasKey(CAMERA_ZOOM_SPEED_KEY))
        {
            _zoomSpeed.value = PlayerPrefs.GetFloat(CAMERA_ZOOM_SPEED_KEY);
        }
        //POPUPS
        if (PlayerPrefs.HasKey(POPUPS_TIME_FOR_LOCK_KEY))
        {
            _popupsTimeForLock.value = PlayerPrefs.GetFloat(POPUPS_TIME_FOR_LOCK_KEY);
        }
        if (PlayerPrefs.HasKey(POPUPS_DURATION_HOVER_KEY))
        {
            _popupsDurationHover.value = PlayerPrefs.GetFloat(POPUPS_DURATION_HOVER_KEY);
        }
        if (PlayerPrefs.HasKey(POPUPS_DURATION_SURVIVING_KEY))
        {
            _popupsDurationSurviving.value = PlayerPrefs.GetFloat(POPUPS_DURATION_SURVIVING_KEY);
        }
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
        PlayerPrefs.SetFloat(CAMERA_MOVEMENT_SPEED_KEY, value);
        PlayerPrefs.Save();
    }

    public void CameraDragSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.CameraDragSpeed = value;
        }
        PlayerPrefs.SetFloat(CAMERA_DRAG_SPEED_KEY, value);
        PlayerPrefs.Save();
    }

    public void CameraEdgePanSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.EdgePanSpeed = value;
        }
        PlayerPrefs.SetFloat(CAMERA_EDGE_PAN_SPEED_KEY, value);
        PlayerPrefs.Save();
    }

    public void CameraZoomSpeedUpdate(Single value)
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.CameraZoomSpeed = value;
        }
        PlayerPrefs.SetFloat(CAMERA_ZOOM_SPEED_KEY, value);
        PlayerPrefs.Save();
    }
    #endregion

    #region POPUPS
    public void PopUpTimeForLockUpdate(Single value)
    {
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.DurationForLockingPopup = value;
        }
        PlayerPrefs.SetFloat(POPUPS_TIME_FOR_LOCK_KEY, value);
        PlayerPrefs.Save();
    }

    public void PopUpTimeForSpawnUpdate(Single value)
    {
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.DurationHoverForUI = value;
        }
        PlayerPrefs.SetFloat(POPUPS_DURATION_HOVER_KEY, value);
        PlayerPrefs.Save();
    }

    public void PopUpSurvivingDurationUpdate(Single value)
    {
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.SurvivablePopupDuration = value;
        }
        PlayerPrefs.SetFloat(POPUPS_DURATION_SURVIVING_KEY, value);
        PlayerPrefs.Save();
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

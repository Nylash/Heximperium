using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    [Header("_________________________________________________________")]
    [Header("Audio")]
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _effectsVolume;
    [SerializeField] private Slider _musicVolume;

    protected override void OnAwake()
    {
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
    public void SetResolution()
    {
        if (Screen.fullScreen)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
        }
    }

    public void SetFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
    #endregion
}

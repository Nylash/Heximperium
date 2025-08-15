using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _gameDurationDropdown;

    public TMP_Dropdown GameDurationDropdown { get => _gameDurationDropdown; }

    private void Start()
    {
        Application.targetFrameRate = 60;
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void LaunchGame(string targetScene)
    {
        FindAnyObjectByType<LoadingManager>().StartLoading(targetScene);
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void QuitGame()
    {
        Application.OpenURL("https://forms.gle/oGGde8EdEBiKebiY9");
        Application.Quit();
    }

    public void OnDropdownChanged(int index)
    {
        string label = _gameDurationDropdown.options[index].text;

        string digits = string.Concat(label.Where(char.IsDigit));
        int value = int.Parse(digits);

        LoadingManager.Instance.GameDuration = value;
    }
}

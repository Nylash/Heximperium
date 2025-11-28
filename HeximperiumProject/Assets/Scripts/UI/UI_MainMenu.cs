using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _bestScore;

    private void Start()
    {
        Application.targetFrameRate = 60;
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (bestScore > 0)
            _bestScore.text = $"Best Score: {bestScore}<sprite name=\"Point_Emoji\">";
    }

    public void LaunchGame(string targetScene)
    {
        FindAnyObjectByType<LoadingManager>().StartLoading(targetScene);
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

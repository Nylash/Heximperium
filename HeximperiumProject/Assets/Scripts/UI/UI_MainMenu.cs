using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
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

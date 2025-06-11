using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public void LaunchGame()
    {
        SceneManager.LoadScene("Test");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
